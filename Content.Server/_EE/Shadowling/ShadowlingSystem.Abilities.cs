using System.Numerics;
using Content.Server.EntityEffects.Effects;
using Content.Server.Storage.Components;
using Content.Shared._EE.Shadowling;
using Content.Shared.Clothing.Components;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Popups;
using Content.Shared.Singularity;
using Microsoft.CodeAnalysis.Operations;
using Robust.Shared.Prototypes;


namespace Content.Server._EE.Shadowling;


/// <summary>
/// This handles...
/// </summary>
public sealed partial class ShadowlingSystem
{
    public void SubscribeAbilities()
    {
        SubscribeLocalEvent<ShadowlingComponent, HatchEvent>(OnHatch);
    }

    # region Hatch

    private void OnHatch(EntityUid uid, ShadowlingComponent comp, HatchEvent args)
    {
        _actions.RemoveAction(uid, args.Action);

        StartHatchingProgress(uid, comp);

        comp.CurrentPhase = ShadowlingPhases.PostHatch;
    }

    private void StartHatchingProgress(EntityUid uid, ShadowlingComponent comp)
    {
        comp.IsHatching = true;

        // Shadowlings change skin colour once hatched
        if (TryComp<HumanoidAppearanceComponent>(uid, out var appearance))
        {
            appearance.EyeColor = comp.EyeColor;
            appearance.SkinColor = comp.SkinColor;
            Dirty(uid, appearance);
        }

        var egg = SpawnAtPosition(comp.Egg, Transform(uid).Coordinates);
        if (TryComp<HatchingEggComponent>(egg, out var eggComp) &&
            TryComp<EntityStorageComponent>(egg, out var eggStorage))
        {
            eggComp.ShadowlingInside = uid;
            // Put shadowling inside
            _entityStorage.Insert(uid, egg, eggStorage);
        }

        // Shadowling shouldn't be able to take damage during this process.
    }

    #endregion
}
