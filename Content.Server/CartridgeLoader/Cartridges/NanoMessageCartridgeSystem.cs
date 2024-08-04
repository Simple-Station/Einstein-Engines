using System.Diagnostics;
using System.Linq;
using Content.Server.NanoMessage;
using Content.Server.NanoMessage.Events;
using Content.Shared.CartridgeLoader;
using Content.Shared.CartridgeLoader.Cartridges;
using Content.Shared.IdentityManagement;
using Content.Shared.NanoMessage.Data;
using Content.Shared.NanoMessage.Events;
using Content.Shared.NanoMessage.Events.Cartridge;
using Content.Shared.PDA;
using Robust.Shared.Utility;

namespace Content.Server.CartridgeLoader.Cartridges;

public sealed class NanoMessageCartridgeSystem : EntitySystem
{
    [Dependency] private readonly CartridgeLoaderSystem _cartridgeLoader = default!;
    [Dependency] private readonly NanoMessageClientSystem _clients = default!;
    [Dependency] private readonly NanoMessageServerSystem _servers = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NanoMessageServerComponent, NanoMessageClientsChangedEvent>(OnServerClientsChanged);
        SubscribeLocalEvent<NanoMessageCartridgeComponent, CartridgeUiReadyEvent>(OnUiReady);
        SubscribeLocalEvent<NanoMessageCartridgeComponent, CartridgeMessageEvent>(OnUiMessage);
        SubscribeLocalEvent<NanoMessageCartridgeComponent, CartridgeAfterInteractEvent>(OnAfterInteract);
    }

    private void OnServerClientsChanged(Entity<NanoMessageServerComponent> ent, ref NanoMessageClientsChangedEvent args)
    {
        var recipients = _servers.GetClientsData(ent!).ToList();

        // Update each cartridge's ui. This can be pretty costly, but shouldn't happen too often.
        foreach (var client in ent.Comp.ConnectedClients)
        {
            if (!TryComp<NanoMessageCartridgeComponent>(client.Ent, out var cartridge))
                continue;

            RefreshClientData((client.Ent, cartridge), recipients);
            UpdateUiState((client.Ent, cartridge));
        }
    }

    private void OnUiReady(Entity<NanoMessageCartridgeComponent> ent, ref CartridgeUiReadyEvent args)
    {
        UpdateUiState(ent!, args.Loader);
    }

    private void OnUiMessage(Entity<NanoMessageCartridgeComponent> ent, ref CartridgeMessageEvent args)
    {
        // This is dumb.
        switch (args)
        {
            case NanoMessageCartridgeAddRecipientRequest addRecipient:
                OnAddRecipientRequest(ent, ref addRecipient);
                break;
            case NanoMessageReconnectRequest reconnect:
                OnReconnect(ent, ref reconnect);
                break;
            case NanoMessageMessageSendRequest send:
                // TODO
                break;
            case NanoMessageChooseConversationRequest choose:
                // TODO
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnAddRecipientRequest(Entity<NanoMessageCartridgeComponent> ent, ref NanoMessageCartridgeAddRecipientRequest args)
    {
        if (args.Id <= 0
            || ent.Comp.KnownRecipients.Contains(args.Id)
            || !TryComp<NanoMessageClientComponent>(ent, out var client)
            || client.ConnectedServer is not { Valid: true } server)
            return;

        ent.Comp.KnownRecipients.Add(args.Id);
        RefreshClientData(ent, _servers.GetClientsData(server).ToList());
        UpdateUiState(ent!);
    }

    private void OnReconnect(Entity<NanoMessageCartridgeComponent> ent, ref NanoMessageReconnectRequest args)
    {
        if (!TryComp<NanoMessageClientComponent>(ent, out var client))
            return;

        _clients.TryReconnect((ent.Owner, client));
        UpdateUiState(ent!);
    }

    // TODO
    // private void OnSendMessage(Entity<NanoMessageCartridgeComponent> ent, ref NanoMessageMessageSendRequest args)
    // private void OnChooseConversation(Entity<NanoMessageCartridgeComponent> ent, ref NanoMessageChooseConversationRequest args)

    /// <summary>
    ///     When touching another NanoMessage client or PDA, try to add it to the recipient list.
    /// </summary>
    private void OnAfterInteract(Entity<NanoMessageCartridgeComponent> ent, ref CartridgeAfterInteractEvent args)
    {
        if (!args.InteractEvent.CanReach || !args.InteractEvent.Target.HasValue
            || !TryComp<NanoMessageClientComponent>(ent, out var thisClient))
            return;

        var target = args.InteractEvent.Target.Value;
        Entity<NanoMessageClientComponent>? targetClient = null;

        // If the target is a standalone client, use its ID.
        if (TryComp<NanoMessageClientComponent>(target, out var standaloneClient))
            targetClient = (target, standaloneClient);

        // If the target is a PDA, try to locate a NanoMessage cartridge and use its ID.
        if (targetClient is not { } && TryComp<CartridgeLoaderComponent>(target, out var loader))
        {
            var cartridge = _cartridgeLoader.GetAvailablePrograms(target, loader)
                .Select(GetEntity)
                .Select(CompOrNull<NanoMessageClientComponent>)
                .FirstOrDefault(it => it is not null);

            if (cartridge is not null)
                targetClient = (cartridge.Owner, cartridge);
        }

        if (targetClient is not { } otherClient || otherClient.Comp.Id <= 0)
            return;

        // Add the recipient if it wasn't previously added
        if (!ent.Comp.KnownRecipients.Contains(otherClient.Comp.Id))
        {
            ent.Comp.KnownRecipients.Add(otherClient.Comp.Id);
            UpdateAll((ent.Owner, ent.Comp, thisClient));
        }

        // Also, if the target is a cartridge, add this client to its recipient list if it's not already there
        if (TryComp<NanoMessageCartridgeComponent>(otherClient.Owner, out var targetCartridge) && !targetCartridge.KnownRecipients.Contains(thisClient.Id))
        {
            targetCartridge.KnownRecipients.Add(thisClient.Id);
            UpdateAll((otherClient.Owner, targetCartridge, otherClient.Comp));
        }

        // TODO: popup


    }

    public void UpdateAll(Entity<NanoMessageCartridgeComponent?, NanoMessageClientComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp1, false) || !Resolve(ent, ref ent.Comp2, false))
            return;

        if (ent.Comp2.ConnectedServer is { Valid: true } server)
            RefreshClientData(ent, _servers.GetClientsData(server).ToList());

        UpdateUiState((ent.Owner, ent.Comp1));
    }

    private void UpdateUiState(Entity<NanoMessageCartridgeComponent?> ent, EntityUid? loader = null)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        if (loader == null)
        {
            if (TryComp<CartridgeComponent>(loader, out var cartridgeComp))
                loader = cartridgeComp.LoaderUid;
            else
                return;
        }

        var client = EnsureComp<NanoMessageClientComponent>(ent);
        if (!_clients.IsServerValid((ent.Owner, client), client.ConnectedServer) && !_clients.TryReconnect((ent.Owner, client)))
            return;

        NanoMessageConversation? currentConvo = null;
        if (ent.Comp.CurrentRecipient != null)
            _servers.TryConversationBetween(client.ConnectedServer, ent.Comp.CurrentRecipient.Value, client.Id, out currentConvo);

        var state = new NanoMessageUiState
        {
            ConnectedServerLabel = client.ConnectedServer is { Valid: true } ? Identity.Name(client.ConnectedServer, EntityManager) : null,
            KnownRecipients = ent.Comp.KnownRecipientsData,
            OpenedConversation = currentConvo
        };
        _cartridgeLoader?.UpdateCartridgeUiState(loader!.Value, state);
    }

    private void RefreshClientData(Entity<NanoMessageCartridgeComponent> ent, List<NanoMessageRecipient> fullData)
    {
        var knownRecipients = fullData.Where(it => ent.Comp.KnownRecipients.Contains(it.Id)).ToList();
        ent.Comp.KnownRecipientsData = knownRecipients;
    }
}
