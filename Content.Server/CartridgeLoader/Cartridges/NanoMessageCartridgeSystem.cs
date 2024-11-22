using System.Linq;
using Content.Server.NanoMessage;
using Content.Server.NanoMessage.Events;
using Content.Server.Popups;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.CartridgeLoader;
using Content.Shared.CartridgeLoader.Cartridges;
using Content.Shared.IdentityManagement;
using Content.Shared.IdentityManagement.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.NanoMessage.Data;
using Content.Shared.NanoMessage.Events.Cartridge;
using Content.Shared.PDA;
using Robust.Shared.Containers;
using Robust.Shared.Timing;


namespace Content.Server.CartridgeLoader.Cartridges;

public sealed class NanoMessageCartridgeSystem : EntitySystem
{
    [Dependency] private readonly CartridgeLoaderSystem _cartridgeLoader = default!;
    [Dependency] private readonly NanoMessageClientSystem _clients = default!;
    [Dependency] private readonly NanoMessageServerSystem _servers = default!;
    [Dependency] private readonly PopupSystem _popups = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NanoMessageServerComponent, NanoMessageClientsChangedEvent>(OnServerClientsChanged);
        SubscribeLocalEvent<NanoMessageCartridgeComponent, CartridgeAddedEvent>(OnInstall);
        SubscribeLocalEvent<NanoMessageCartridgeComponent, CartridgeUiReadyEvent>(OnUiReady);
        SubscribeLocalEvent<NanoMessageCartridgeComponent, CartridgeMessageEvent>(OnUiMessage);
        SubscribeLocalEvent<NanoMessageCartridgeComponent, CartridgeAfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<NanoMessageCartridgeComponent, NanoMessageClientMessageReceiveEvent>(OnReceiveMessage);
    }

    private void OnServerClientsChanged(Entity<NanoMessageServerComponent> ent, ref NanoMessageClientsChangedEvent args)
    {
        var recipients = _servers.GetClientsData(ent!).ToList();

        // Update each cartridge's ui. This can be pretty costly, but shouldn't happen too often.
        foreach (var client in ent.Comp.ConnectedClients)
        {
            if (!TryComp<NanoMessageCartridgeComponent>(client.Ent, out var cartridge))
                continue;

            UpdateClientData((client.Ent, cartridge), recipients);
            UpdateUiState((client.Ent, cartridge));
        }
    }

    private void OnInstall(Entity<NanoMessageCartridgeComponent> ent, ref CartridgeAddedEvent args)
    {
        // Update each cartridge's preferred name if the PDA is worn
        var loader = args.Loader;
        if (!TryComp<PdaComponent>(loader, out var pdaComp)
            || !TryComp<NanoMessageClientComponent>(ent, out var cartridgeClient)
            || TryComp<CartridgeComponent>(ent, out var cartridge) && cartridge.InstallationStatus == InstallationStatus.Cartridge)
            return;

        // The task is delayed because the entity name may not be set at this point
        Timer.Spawn(0, () =>
        {
            // Try to find the entity wearing this PDA and use its name. If there's no such entity, try to inherit it from the ID card.
            string? name = null;
            if (_container.TryGetOuterContainer(loader, Transform(loader), out var inventoryContainer)
                && inventoryContainer.Owner is var pdaHolder
                && HasComp<MobStateComponent>(pdaHolder))
            {
                name = Identity.Name(pdaHolder, EntityManager);
            }
            else if (pdaComp.ContainedId is { } idCard && TryComp<IdCardComponent>(idCard, out var idComp))
            {
                name = idComp.FullName
                    ?? Loc.GetString("nano-message-cartridge-pda-default-job-name", ("job", idComp.JobTitle ?? "unknown"));
            }

            if (name is null)
                return;

            cartridgeClient.PreferredName = name;
            _servers.UpdateClientData(cartridgeClient.ConnectedServer, (ent, cartridgeClient));
        });
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
                OnSendMessage(ent, ref send);
                break;
            case NanoMessageChooseConversationRequest choose:
                OnChooseConversation(ent, ref choose);
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
        UpdateAll((ent.Owner, ent.Comp, client));
    }

    private void OnReconnect(Entity<NanoMessageCartridgeComponent> ent, ref NanoMessageReconnectRequest args)
    {
        if (!TryComp<NanoMessageClientComponent>(ent, out var client))
            return;

        _clients.TryReconnect((ent.Owner, client));
        UpdateUiState(ent!);
    }

    private void OnSendMessage(Entity<NanoMessageCartridgeComponent> ent, ref NanoMessageMessageSendRequest args)
    {
        if (!TryComp<NanoMessageClientComponent>(ent, out var client)
            || client.ConnectedServer is not { Valid: true } server
            || !_servers.TryConversationBetween(server, client.Id, args.RecipientId, out var conv, createIfMissing: true))
            return;

        _clients.TrySendMessage((ent, client), conv.Value.Id, args.Message);
        UpdateUiState(ent!);
    }

    private void OnChooseConversation(Entity<NanoMessageCartridgeComponent> ent, ref NanoMessageChooseConversationRequest args)
    {
        // TODO: show an error popup or a fallback conversation or whatever
        if (!TryComp<NanoMessageClientComponent>(ent, out var client)
            || client.ConnectedServer is not { Valid: true } server
            || !_servers.TryConversationBetween(server, client.Id, args.RecipientId, out var conv, createIfMissing: true))
            return;

        ent.Comp.CurrentConversationId = conv.Value.Id;
        UpdateUiState(ent!);
    }

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

        _popups.PopupEntity(Loc.GetString("nano-message-popup-paired", ("used", args.InteractEvent.Used), ("target", target)), ent);
    }

    private void OnReceiveMessage(Entity<NanoMessageCartridgeComponent> ent, ref NanoMessageClientMessageReceiveEvent args)
    {
        UpdateUiState(ent!);
        if (!TryComp<NanoMessageClientComponent>(ent, out var client)
            || args.Message.Sender == client.Id) // Don't send a notification if this message is from self
            return;

        var senderName = GetClientName(ent, args.Message.Sender);
        var header = Loc.GetString("nano-message-notification-message-header", ("sender", senderName));

        if (TryComp<CartridgeComponent>(ent, out var cartridgeComponent) && cartridgeComponent.LoaderUid is { } loader)
            _cartridgeLoader.SendNotification(loader, header, args.Message.Content);
    }

    /// <summary>
    ///     Updates the ui state and refreshes the client data of the given client.
    /// </summary>
    public void UpdateAll(Entity<NanoMessageCartridgeComponent?, NanoMessageClientComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp1, false) || !Resolve(ent, ref ent.Comp2, false))
            return;

        if (ent.Comp2.ConnectedServer is { Valid: true } server)
            UpdateClientData(ent!, _servers.GetClientsData(server).ToList());

        UpdateUiState((ent.Owner, ent.Comp1));
    }

    /// <summary>
    ///     Updates the ui state of the given client, without refreshing the client data.
    /// </summary>
    public void UpdateUiState(Entity<NanoMessageCartridgeComponent?> ent, EntityUid? loader = null)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        if (loader == null)
        {
            if (TryComp<CartridgeComponent>(ent, out var cartridgeComp))
                loader = cartridgeComp.LoaderUid;
            else
                return;
        }

        var client = EnsureComp<NanoMessageClientComponent>(ent);
        if (!_clients.IsServerValid((ent.Owner, client), client.ConnectedServer) && !_clients.TryReconnect((ent.Owner, client)))
            return;

        NanoMessageConversation? currentConvo = null;
        if (ent.Comp.CurrentConversationId is { } currentConversationId)
            _servers.TryConversation(client.ConnectedServer, currentConversationId, out currentConvo);

        var state = new NanoMessageUiState
        {
            ConnectedServerLabel = client.ConnectedServer is { Valid: true } ? Identity.Name(client.ConnectedServer, EntityManager) : null,
            KnownRecipients = ent.Comp.KnownRecipientsData,
            OpenedConversation = currentConvo
        };
        _cartridgeLoader.UpdateCartridgeUiState(loader!.Value, state);
    }

    private void UpdateClientData(Entity<NanoMessageCartridgeComponent> ent, List<NanoMessageRecipient> fullData)
    {
        var knownRecipients = fullData.Where(it => ent.Comp.KnownRecipients.Contains(it.Id)).ToList();
        ent.Comp.KnownRecipientsData = knownRecipients;
    }

    private string GetClientName(Entity<NanoMessageCartridgeComponent> ent, ulong id)
    {
        var result = ent.Comp.KnownRecipientsData
            .Where(it => it.Id == id)
            .Cast<NanoMessageRecipient?>()
            .FirstOrDefault();

        return result?.Name ?? Loc.GetString("nano-message-unknown-user-short");
    }
}
