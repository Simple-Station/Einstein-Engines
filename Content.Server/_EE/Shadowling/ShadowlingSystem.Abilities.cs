using System.Numerics;
using Content.Server.EntityEffects.Effects;
using Content.Shared._EE.Shadowling;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Popups;
using Content.Shared.Singularity;


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

        // Hide Player Entity (idk if there's a better way), and Change Skin Color
        _humanoid.SetScale(uid, Vector2.Zero);
        _humanoid.SetSkinColor(uid, Color.Black);

        if (TryComp<HumanoidAppearanceComponent>(uid, out var appearance))
        {
            appearance.EyeColor = comp.EyeColor;
            Dirty(uid, appearance);
        }

        // No Movement
        _movementSpeed.ChangeBaseSpeed(uid, 0, 0, 0);

        // Spawn Egg Entity

        // Start Timer


        // Reset Values
        // _humanoid.SetScale(uid, Vector2.One);
        // comp.IsHatching = false;
        // _movementSpeed.RefreshMovementSpeedModifiers(uid);

        // Shadowling shouldn't be able to take damage during this process.
    }

    #endregion
}
