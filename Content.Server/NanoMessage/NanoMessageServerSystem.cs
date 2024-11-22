using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.NanoMessage.Events;
using Content.Server.Power.Components;
using Content.Shared.CartridgeLoader.Cartridges;
using Content.Shared.NanoMessage.Data;
using Robust.Shared.Utility;

namespace Content.Server.NanoMessage;

public sealed partial class NanoMessageServerSystem : EntitySystem
{
    public bool IsServerActive(Entity<NanoMessageServerComponent?> server)
    {
        if (!Resolve(server, ref server.Comp))
            return false;

        return server.Comp.Enabled && (!TryComp<ApcPowerReceiverComponent>(server, out var apc) || apc.Powered);
    }

    #region Connections

    public bool IsConnected(Entity<NanoMessageServerComponent?> server, EntityUid client)
    {
        if (!Resolve(server, ref server.Comp))
            return false;

        return server.Comp.ConnectedClients.Any(c => c.Ent == client);
    }

    public bool TryConnect(Entity<NanoMessageServerComponent?> server, Entity<NanoMessageClientComponent?> client)
    {
        if (!Resolve(server, ref server.Comp) || !Resolve(client, ref client.Comp) || !IsServerActive(server))
            return false;

        var attempt = new NanoMessageConnectAttemptEvent { Server = server!, Client = client! };
        RaiseLocalEvent(server, ref attempt);
        RaiseLocalEvent(client, ref attempt);

        if (attempt.Cancelled)
            return false;

        client.Comp.ConnectedServer = server;
        server.Comp.ConnectedClients.RemoveAll(c => c.Ent == client.Owner);
        server.Comp.ConnectedClients.Add((client.Owner, client.Comp.Id));

        if (!server.Comp.ClientData.ContainsKey(client.Comp.Id))
            server.Comp.ClientData[client.Comp.Id] = ExtractData(client!, null);

        RaiseLocalEvent(server, new NanoMessageClientsChangedEvent());

        return true;
    }

    public void Disconnect(Entity<NanoMessageServerComponent?> server, Entity<NanoMessageClientComponent?> client)
    {
        if (!Resolve(server, ref server.Comp) || !Resolve(client, ref client.Comp))
            return;

        server.Comp.ConnectedClients.RemoveAll(c => c.Ent == client.Owner);
        if (client.Comp.ConnectedServer == server.Owner)
            client.Comp.ConnectedServer = EntityUid.Invalid;

        RaiseLocalEvent(server, new NanoMessageClientsChangedEvent());
    }

    #endregion

    #region Client Data

    public IEnumerable<NanoMessageRecipient> GetClientsData(Entity<NanoMessageServerComponent?> server)
    {
        if (!Resolve(server, ref server.Comp))
            return [];

        return server.Comp.ConnectedClients
            .Where(c => server.Comp.ClientData.ContainsKey(c.Id))
            .Select(c => server.Comp.ClientData[c.Id]);
    }

    public void UpdateClientData(Entity<NanoMessageServerComponent?> server, Entity<NanoMessageClientComponent?> client)
    {
        if (!Resolve(server, ref server.Comp) || !Resolve(client, ref client.Comp))
            return;

        NanoMessageRecipient? oldData = server.Comp.ClientData.TryGetValue(client.Comp.Id, out var old) ? old : null;
        server.Comp.ClientData[client.Comp.Id] = ExtractData(client!, oldData);
    }

    private NanoMessageRecipient ExtractData(Entity<NanoMessageClientComponent> client, NanoMessageRecipient? oldData)
    {
        return new()
        {
            Id = client.Comp.Id,
            Name = oldData?.CustomNameOverridden == true ? oldData?.Name : client.Comp.PreferredName
        };
    }

    #endregion

    /// <summary>
    ///     Tries to find a client with the given id on the given server.
    /// </summary>
    public bool TryFindClient(Entity<NanoMessageServerComponent?> server, ulong id, [NotNullWhen(true)] out Entity<NanoMessageClientComponent>? result)
    {
        result = null;
        if (!Resolve(server, ref server.Comp))
            return false;

        var clientId = server.Comp.ConnectedClients
            .Where(c => c.Id == id)
            .Select(c => c.Ent)
            .Cast<EntityUid?>()
            .FirstOrDefault();

        if (clientId is null || !TryComp<NanoMessageClientComponent>(clientId, out var clientComp))
            return false;

        result = (clientId.Value, clientComp);
        return true;
    }

    #region Conversations

    /// <summary>
    ///     Tries to find or create a conversation between two users.
    ///     Always fails if one of the users is not connected, or if the server is inactive, unless forced.
    /// </summary>
    public bool TryConversationBetween(
        Entity<NanoMessageServerComponent?> server,
        ulong user1, ulong user2,
        [NotNullWhen(true)] out NanoMessageConversation? conversation,
        bool createIfMissing = false,
        bool force = false
    )
    {
        conversation = null;
        if (!Resolve(server, ref server.Comp))
            return false;

        if (!force &&
            (!IsServerActive(server)
            || !server.Comp.ConnectedClients.Any(it => it.Id == user1)
            || !server.Comp.ConnectedClients.Any(it => it.Id == user2)))
            return false;

        conversation = server.Comp.Conversations
            .Where(c => (c.User1 == user1 && c.User2 == user2) || (c.User1 == user2 && c.User2 == user1))
            .Cast<NanoMessageConversation?>()
            .FirstOrDefault();

        if (conversation == null && createIfMissing)
        {
            conversation = new()
            {
                Id = NextConversationId(server!),
                Messages = [],
                User1 = user1,
                User2 = user2
            };
            server.Comp.Conversations.Add(conversation.Value);
        }

        return conversation != null;
    }

    /// <summary>
    ///     Tries to find a conversation with the given id. Fails if the server is inactive.
    /// </summary>
    public bool TryConversation(Entity<NanoMessageServerComponent?> server, ulong id, [NotNullWhen(true)] out NanoMessageConversation? conversation, bool force = false)
    {
        conversation = null;
        if (!Resolve(server, ref server.Comp) || !(force || IsServerActive(server)))
            return false;

        conversation = server.Comp.Conversations
            .Where(c => c.Id == id)
            .Cast<NanoMessageConversation?>()
            .FirstOrDefault();

        return conversation != null;
    }

    /// <summary>
    ///     Tries to dispatch the given message in the context of the given conversation.
    /// </summary>
    public bool TryDispatchMessage(Entity<NanoMessageServerComponent?> server, ulong conversationId, NanoMessageMessage message)
    {
        if (!Resolve(server, ref server.Comp)
            || !IsServerActive(server)
            || !TryConversation(server, conversationId, out var conv, force: true))
            return false;

        var serverMessageEv = new NanoMessageMessageDispatchAttemptEvent(conv.Value, message);
        RaiseLocalEvent(server.Owner, ref serverMessageEv);

        if (serverMessageEv.Cancelled)
            return false;

        conv.Value.Messages.Add(message);

        var clientMessageEv = new NanoMessageClientMessageReceiveEvent(conv.Value, message);
        if (TryFindClient(server, conv.Value.User1, out var client1))
            RaiseLocalEvent(client1.Value, clientMessageEv);

        if (TryFindClient(server, conv.Value.User2, out var client2))
            RaiseLocalEvent(client2.Value, clientMessageEv);

        return true;
    }

    private ulong NextConversationId(Entity<NanoMessageServerComponent> server)
    {
        return server.Comp.NextConversationId++;
    }

    #endregion
}
