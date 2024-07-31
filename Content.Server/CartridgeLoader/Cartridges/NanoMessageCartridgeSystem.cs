using System.Linq;
using Content.Server.NanoMessage;
using Content.Server.NanoMessage.Events;
using Content.Server.Power.Components;
using Content.Shared.CartridgeLoader;
using Content.Shared.CartridgeLoader.Cartridges;
using Content.Shared.IdentityManagement;

namespace Content.Server.CartridgeLoader.Cartridges;

public sealed class NanoMessageCartridgeSystem : EntitySystem
{
    [Dependency] private readonly CartridgeLoaderSystem? _cartridgeLoaderSystem = default!;
    [Dependency] private readonly NanoMessageClientSystem _clients = default!;
    [Dependency] private readonly NanoMessageServerSystem _servers = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NanoMessageServerComponent, NanoMessageClientsChangedEvent>(OnServerClientsChanged);
        SubscribeLocalEvent<NanoMessageCartridgeComponent, CartridgeUiReadyEvent>(OnUiReady);
        //SubscribeLocalEvent<NanoMessageCatridgeComponent, CartridgeMessageEvent>(OnUiMessage);
    }

    private void OnServerClientsChanged(Entity<NanoMessageServerComponent> ent, ref NanoMessageClientsChangedEvent args)
    {
        var recipients = _servers.GetClientsData(ent).ToList();

        // Update each cartridge's ui. This can be pretty costly, but shouldn't happen too often.
        foreach (var client in ent.Comp.ConnectedClients)
        {
            if (!TryComp<NanoMessageCartridgeComponent>(client.Ent, out var cartridge))
                continue;

            var knownRecipients = recipients.Where(it => cartridge.KnownRecipients.Contains(it.Id)).ToList();
            cartridge.KnownRecipientsData = knownRecipients;

            UpdateUiState((client.Ent, cartridge));
        }
    }

    private void OnUiReady(Entity<NanoMessageCartridgeComponent> ent, ref CartridgeUiReadyEvent args)
    {
        UpdateUiState(ent!, args.Loader);
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

        var currentConvo = ent.Comp.CurrentRecipient != null
            && _servers.TryConversationBetween(client.ConnectedServer, ent.Comp.CurrentRecipient.Value, client.Id, out var conversation)
            ? conversation
            : null;

        var state = new NanoMessageUiState
        {
            ConnectedServerLabel = client.ConnectedServer is { Valid: true } ? Identity.Name(client.ConnectedServer, EntityManager) : null,
            KnownRecipients = ent.Comp.KnownRecipientsData,
            OpenedConversation = currentConvo
        };
        _cartridgeLoaderSystem?.UpdateCartridgeUiState(loader!.Value, state);
    }
}
