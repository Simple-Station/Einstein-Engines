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
            server.Comp.ClientData[client.Comp.Id] = new NanoMessageRecipient { Id = client.Comp.Id };

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

    public IEnumerable<NanoMessageRecipient> GetClientsData(Entity<NanoMessageServerComponent?> server)
    {
        if (!Resolve(server, ref server.Comp))
            return [];

        return server.Comp.ConnectedClients
            .Where(c => server.Comp.ClientData.ContainsKey(c.Id))
            .Select(c => server.Comp.ClientData[c.Id]);
    }

    public bool TryConversationBetween(
        Entity<NanoMessageServerComponent?> server,
        ulong user1, ulong user2,
        [NotNullWhen(true)] out NanoMessageConversation? conversation
    )
    {
        conversation = null;
        if (!Resolve(server, ref server.Comp))
            return false;

        conversation = server.Comp.Conversations
            .FirstOrDefault(c => (c.User1 == user1 && c.User2 == user2) || (c.User1 == user2 && c.User2 == user1));

        return conversation != null;
    }
}
