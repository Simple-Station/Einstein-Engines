using Content.Shared.Abilities.Psionics;
using Content.Shared.Psionics;
using Content.Shared.Random;
using Robust.Shared.Prototypes;

namespace Content.Server.Psionics;

public sealed partial class BasicPsionicPowerListSystem : EntitySystem
{
    [Dependency] private readonly SharedPsionicAbilitiesSystem _psionicAbilities = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BasicPsionicPowerListComponent, PsionicInitEvent>(OnStartup);
    }

    private void OnStartup(EntityUid uid, BasicPsionicPowerListComponent component, PsionicInitEvent args) =>
        _psionicAbilities.GenerateAvailablePowers(args.PsionicComponent, component.PowerPool.Id);
}

[RegisterComponent]
public sealed partial class BasicPsionicPowerListComponent : Component
{
    /// <summary>
    ///     The list of powers that this Psion is eligible to roll new abilities from.
    ///     This generates the initial ability pool, but can also be modified by other systems.
    /// </summary>
    [DataField]
    public ProtoId<WeightedRandomPrototype> PowerPool = "RandomPsionicPowerPool";
}
