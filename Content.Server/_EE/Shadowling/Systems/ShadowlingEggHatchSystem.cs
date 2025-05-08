using Content.Server.Polymorph.Systems;
using Content.Server.Popups;
using Content.Server.Storage.Components;
using Content.Server.Storage.EntitySystems;
using Content.Shared._EE.Clothing.Components;
using Content.Shared._EE.Shadowling;
using Content.Shared.Popups;
using Robust.Shared.Timing;


namespace Content.Server._EE.Shadowling;


/// <summary>
/// This handles the hatching process
/// </summary>
///
public sealed class ShadowlingEggHatchSystem : EntitySystem
{
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly EntityStorageSystem _entityStorage = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var query = EntityQueryEnumerator<HatchingEggComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            var sUid = comp.ShadowlingInside;

            # region Message Popups

            if (comp.CooldownTimer <= 12 && !comp.HasFirstMessageAppeared)
            {
                _popupSystem.PopupEntity(Loc.GetString("sling-hatch-first"), uid, sUid, PopupType.Medium);
                comp.HasFirstMessageAppeared = true;
            }

            if (comp.CooldownTimer <= 7 && !comp.HasSecondMessageAppeared)
            {
                _popupSystem.PopupEntity(Loc.GetString("sling-hatch-second"), uid, sUid, PopupType.Medium);
                comp.HasSecondMessageAppeared = true;
            }

            if (comp.CooldownTimer <= 3 && !comp.HasThirdMessageAppeared)
            {
                _popupSystem.PopupEntity(Loc.GetString("sling-hatch-third"), uid, sUid, PopupType.Medium);
                comp.HasThirdMessageAppeared = true;
            }

            #endregion

            comp.CooldownTimer -= frameTime;
            if (comp.CooldownTimer <= 0)
                Cycle(sUid, uid, comp);
        }
    }

    public void Cycle(EntityUid sling, EntityUid egg, HatchingEggComponent comp)
    {
        if (comp.HasBeenHatched)
            return;

        if (!TryComp<ShadowlingComponent>(sling, out var shadowling))
            return;

        // Remove sling from egg
        if (TryComp<EntityStorageComponent>(egg, out var storage))
        {
            _entityStorage.Remove(sling, egg, storage);
            _entityStorage.OpenStorage(egg, storage);
        }

        var newUid = _polymorph.PolymorphEntity(sling, shadowling.ShadowlingPolymorphId);
        if (newUid == null)
            return;

        var ascendantShadowlingComp = EntityManager.GetComponent<ShadowlingComponent>(newUid.Value);
        var shadowlingSystem = EntityManager.System<ShadowlingSystem>();

        shadowlingSystem.OnPhaseChanged(newUid.Value, ascendantShadowlingComp, ShadowlingPhases.PostHatch);

        comp.HasBeenHatched = true;
    }
}
