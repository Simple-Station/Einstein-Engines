using Content.Client._Crescent.Broadcaster;
using Content.Client.Bank.UI;
using Content.Shared._Crescent.Broadcaster;
using Content.Shared.Bank.BUI;
using Content.Shared.Bank.Events;
using Content.Shared.NamedModules.Components;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface.XAML;

namespace Content.Client._Crescent.Broadcaster;

public sealed class BroadcasterBui : BoundUserInterface
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    private BroadcasterUI? _menu;

    public BroadcasterBui(EntityUid owner, Enum uiKey) : base(owner, uiKey) { }

    protected override void Open()
    {
        base.Open();
        if (!(_entManager.TryGetComponent<BroadcastingConsoleComponent>(Owner, out var broadcastingComp) &&
              broadcastingComp is not null))
            return;

        _menu = new BroadcasterUI();
        if (broadcastingComp.AvailableAnnouncements is null)
            return;
        _menu.setBroadcastables(broadcastingComp.AvailableAnnouncements);
        _menu.setPlaying(broadcastingComp.currentlyPlaying);
        _menu.ClickBroadcast += OnTryPlayBroadcast;
        _menu.OnClose += Close;
        _menu.OpenCentered();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _menu?.Dispose();
        }
    }

    private void OnTryPlayBroadcast(int index)
    {
        SendMessage(new BroadcasterBroadcastMessage(index));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not BroadcasterConsoleState cast)
            return;

        _menu?.setBroadcastables(cast.playableBroadcasts);
        _menu?.setPlaying(cast.currentlyPlaying);

    }
}
