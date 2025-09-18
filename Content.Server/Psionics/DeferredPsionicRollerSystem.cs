using Content.Shared.Abilities.Psionics;
using Content.Shared.Psionics;
using Timer = Robust.Shared.Timing.Timer;

namespace Content.Server.Psionics;

public sealed partial class DeferredPsionicRollerSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DeferredPsionicRollerComponent, PsiPowersInitEvent>(OnStartup);
    }

    private void OnStartup(EntityUid uid, DeferredPsionicRollerComponent component, PsiPowersInitEvent args) =>
        Timer.Spawn(TimeSpan.FromSeconds(component.Delay), () => DeferRollers(uid));

    /// <summary>
    ///     Open call for any system that wishes to run their power generation on a delay.
    /// </summary>
    private void DeferRollers(EntityUid uid)
    {
        if (!Exists(uid) || !TryComp(uid, out PsionicComponent? component))
            return;

        var deferredRollEvent = new DeferredPsiPowersInitEvent(component);
        RaiseLocalEvent(uid, ref deferredRollEvent);
    }
}

[RegisterComponent]
public sealed partial class DeferredPsionicRollerComponent : Component
{
    [DataField]
    public float Delay = 30f;
}

// No I'm not putting this in shared because the timers are asynchronous.
[ByRefEvent]
public record struct DeferredPsiPowersInitEvent(PsionicComponent PsionicComponent);
