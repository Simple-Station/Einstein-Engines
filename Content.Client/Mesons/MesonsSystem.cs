using Robust.Client.Graphics;
using Robust.Client.Player;
using Content.Shared.Inventory;
using Content.Shared.Mesons;

namespace Content.Client.Mesons;

public sealed class MesonsSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;

    private MesonsOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        _overlay = new();
        _overlayMan.AddOverlay(_overlay);
    }

    public override void Update(float frameTime)
    {
        if (_playerManager.LocalEntity is {} ourUid &&
            _inventory.TryGetSlotEntity(ourUid, "eyes", out EntityUid? slotEntity) &&
            TryComp(slotEntity, out MesonsComponent? mesonsComponent) &&
            mesonsComponent.Enabled)
            _overlay.SetEnabled(true);
        else if (_overlay.Enabled)
            _overlay.SetEnabled(false);
    }
}
