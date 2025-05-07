using Content.Client._Goobstation.Fishing.Overlays;
using Content.Shared._Goobstation.Fishing.Components;
using Content.Shared._Goobstation.Fishing.Systems;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Client._Goobstation.Fishing;

public sealed class FishingSystem : SharedFishingSystem
{
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    public override void Initialize()
    {
        base.Initialize();
        _overlay.AddOverlay(new FishingOverlay(EntityManager, _player));
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _overlay.RemoveOverlay<FishingOverlay>();
    }

    // Does nothing on client, because can't spawn entities in prediction
    protected override void SetupFishingFloat(Entity<FishingRodComponent> fishingRod, EntityUid player, EntityCoordinates target) {}

    // Does nothing on client, because can't delete entities in prediction
    protected override void ThrowFishReward(EntProtoId fishId, EntityUid fishSpot, EntityUid target) {}

    // Does nothing on client, because NUKE ALL PREDICTION!!!! (UseInHands event sometimes gets declined on Server side, and it desyncs, so we can't predict that sadly.
    protected override void CalculateFightingTimings(Entity<ActiveFisherComponent> fisher, ActiveFishingSpotComponent activeSpotComp) {}
}
