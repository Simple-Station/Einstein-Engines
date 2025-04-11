using System.Runtime.InteropServices.Marshalling;
using Content.Server.Popups;
using Content.Server.Storage.Components;
using Content.Server.Storage.EntitySystems;
using Content.Shared._EE.Shadowling;
using Content.Shared.Timing;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Robust.Shared.Timing;


namespace Content.Server._EE.Shadowling;


/// <summary>
/// This handles the hatching process
/// </summary>
///
public sealed class ShadowlingEggHatchSystem : EntitySystem
{
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
                _popupSystem.PopupEntity(Loc.GetString("sling-hatch-first"), uid, comp.ShadowlingInside);
                comp.HasFirstMessageAppeared = true;
            }

            if (comp.CooldownTimer <= 7 && !comp.HasSecondMessageAppeared)
            {
                _popupSystem.PopupEntity(Loc.GetString("sling-hatch-second"), uid, comp.ShadowlingInside);
                comp.HasSecondMessageAppeared = true;
            }

            if (comp.CooldownTimer <= 3 && !comp.HasThirdMessageAppeared)
            {
                _popupSystem.PopupEntity(Loc.GetString("sling-hatch-third"), uid, comp.ShadowlingInside);
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

        if (TryComp<ShadowlingComponent>(sling, out var shadowling))
            shadowling.IsHatching = false;

        // Remove sling from egg
        if (TryComp<EntityStorageComponent>(egg, out var storage))
        {
            _entityStorage.Remove(sling, egg, storage);
            _entityStorage.OpenStorage(egg, storage);
        }

        comp.HasBeenHatched = true;
    }
}
