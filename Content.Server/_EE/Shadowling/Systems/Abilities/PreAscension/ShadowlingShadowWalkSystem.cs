using Content.Server.Actions;
using Content.Server.Popups;
using Content.Server.Stealth;
using Content.Shared._EE.Shadowling;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Stealth.Components;
using Robust.Shared.Timing;


namespace Content.Server._EE.Shadowling;


/// <summary>
/// This handles Shadow Walk!
/// </summary>
public sealed class ShadowlingShadowWalkSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly StealthSystem _stealth = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingShadowWalkComponent, RefreshMovementSpeedModifiersEvent>(OnMove);
        SubscribeLocalEvent<ShadowlingShadowWalkComponent, ShadowWalkEvent>(OnShadowWalk);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var shadowWalkQuery = EntityQueryEnumerator<ShadowlingShadowWalkComponent>();
        while (shadowWalkQuery.MoveNext(out var uid, out var shadowWalk))
        {
            if (shadowWalk.IsActive)
            {
                shadowWalk.Timer -= frameTime;
                if (shadowWalk.Timer <= 0)
                {
                    if (TryComp<StealthComponent>(uid, out var stealth))
                    {
                        _stealth.SetVisibility(uid, 1f, stealth);
                        RemComp<StealthComponent>(uid);
                    }
                    shadowWalk.IsActive = false;
                    _movementSpeedModifier.RefreshMovementSpeedModifiers(uid);
                }
            }
        }
    }

    private void OnMove(EntityUid uid, ShadowlingShadowWalkComponent comp, RefreshMovementSpeedModifiersEvent args)
    {
        if (comp.IsActive)
        {
            args.ModifySpeed(comp.WalkSpeedModifier, comp.RunSpeedModifier);
        }
        else
        {
            args.ModifySpeed(1f, 1f);
        }
    }

    private void OnShadowWalk(EntityUid uid, ShadowlingShadowWalkComponent comp, ShadowWalkEvent args)
    {
        comp.IsActive = true;
        comp.Timer = comp.TimeUntilDeactivation;

        _movementSpeedModifier.RefreshMovementSpeedModifiers(uid);

        // todo: sound
        // todo: visuals

        var stealth = EnsureComp<StealthComponent>(uid);
        _stealth.SetVisibility(uid, 0f, stealth);

        _actions.StartUseDelay(args.Action);
    }
}
