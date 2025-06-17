using Content.Server.Abilities.Psionics;
using Content.Shared.Psionics;
using Robust.Shared.Prototypes;

namespace Content.Server.Psionics;

public sealed partial class InnatePsionicPowersSystem : EntitySystem
{

    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly PsionicAbilitiesSystem _psionicAbilities = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<InnatePsionicPowersComponent, PsiPowersInitEvent>(InnatePowerStartup);
    }

    /// <summary>
    ///     An entity with this component will automatically generate a list of powers, regardless of its available powers list.
    /// </summary>
    private void InnatePowerStartup(EntityUid uid, InnatePsionicPowersComponent comp, PsiPowersInitEvent args)
    {
        var psionic = args.PsionicComponent;

        foreach (var proto in comp.PowersToAdd)
            _psionicAbilities.InitializePsionicPower(uid, _protoMan.Index(proto), psionic, comp.PlayFeedback);
    }
}

[RegisterComponent]
public sealed partial class InnatePsionicPowersComponent : Component
{
    /// <summary>
    ///     The list of all powers to be added on Startup
    /// </summary>
    [DataField]
    public List<ProtoId<PsionicPowerPrototype>> PowersToAdd = new();

    [DataField]
    public bool PlayFeedback;
}
