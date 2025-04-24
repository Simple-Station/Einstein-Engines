using Content.Server.Actions;
using Content.Server.Popups;
using Content.Server.Stealth;
using Content.Shared._EE.Shadowling;
using Content.Shared._EE.Shadowling.Components;
using Content.Shared._EE.Shadowling.Thrall;
using Content.Shared.Popups;
using Content.Shared.Stealth.Components;
using Robust.Shared.Timing;


namespace Content.Server._EE.Shadowling.Thrall;


/// <summary>
/// This handles the Guise ability logic.
/// Guise makes you become invisible for some seconds, ONLY if its activated in the dark.
/// That doesn't mean it doesn't work on light, however.
/// </summary>
public sealed class ThrallGuiseSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly StealthSystem _stealth = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ThrallGuiseComponent, GuiseEvent>(OnGuise);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var query = EntityQueryEnumerator<ThrallGuiseComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Active)
            {
                // We check if the entity was in the shadows before the ability activation, otherwise we cancel it here
                // Its not possible (or fast enough) to do that on ability activation so it gets done here
                if (comp.Timer <= comp.GuiseDuration - 0.25 && !comp.WasInShadows)
                {
                    if (TryComp<LightDetectionComponent>(uid, out var lightDetection))
                    {
                        if (lightDetection.IsOnLight)
                        {
                            _popup.PopupEntity(Loc.GetString("thrall-guise-fail"), uid, uid, PopupType.MediumCaution);
                            comp.Active = false;
                            RemComp<LightDetectionComponent>(uid);
                            RemComp<StealthComponent>(uid);
                            continue;
                        }

                        comp.WasInShadows = true;
                    }
                }
                // Start timer
                comp.Timer -= frameTime;

                if (comp.Timer <= 0)
                {
                    comp.Active = false;
                    if (TryComp<StealthComponent>(uid, out var stealth))
                    {
                        comp.WasInShadows = false;
                        _stealth.SetVisibility(uid, 1f, stealth);
                        RemComp<LightDetectionComponent>(uid);
                        RemComp<StealthComponent>(uid);
                    }
                }
            }
        }
    }

    private void OnGuise(EntityUid uid, ThrallGuiseComponent component, GuiseEvent args)
    {
        EnsureComp<LightDetectionComponent>(uid);
        component.Timer = component.GuiseDuration;
        component.Active = true;

        var stealth = EnsureComp<StealthComponent>(uid);
        _stealth.SetVisibility(uid, -1f, stealth);
        _actions.StartUseDelay(args.Action);
    }
}
