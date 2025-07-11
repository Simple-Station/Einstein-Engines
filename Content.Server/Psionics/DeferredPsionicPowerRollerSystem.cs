using Content.Server.Abilities.Psionics;
using Robust.Shared.Random;

namespace Content.Server.Psionics;

public sealed partial class DeferredPsionicPowerRollerSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly PsionicAbilitiesSystem _psionicAbilities = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DeferredPsionicPowerRollerComponent, DeferredPsiPowersInitEvent>(OnStartup);
    }

    private void OnStartup(EntityUid uid, DeferredPsionicPowerRollerComponent component, DeferredPsiPowersInitEvent args)
    {
        if (!component.AllowExtraPowers & args.PsionicComponent.ActivePowers.Count >= component.MaxPowers
            && !_random.Prob(component.Chance))
            return;

        _psionicAbilities.AddRandomPsionicPower(uid);
    }
}

[RegisterComponent]
public sealed partial class DeferredPsionicPowerRollerComponent : Component
{
    /// <summary>
    ///     The baseline chance of obtaining a psionic power when rolling for one. This component rolls exactly once when initialized. Set it to 1 to guarantee a power.
    /// </summary>
    [DataField]
    public float Chance = 0.15f;

    /// <summary>
    ///     If the user has at least this many powers, the deferred roll will not be made.
    /// </summary>
    [DataField]
    public int MaxPowers = 1;

    /// <summary>
    ///     By default, this component can only give up to a maximum of 1 power, but can optionally go over the limit.
    /// </summary>
    [DataField]
    public bool AllowExtraPowers;
}
