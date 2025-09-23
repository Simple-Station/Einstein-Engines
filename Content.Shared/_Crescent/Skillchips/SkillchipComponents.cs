using Content.Shared.Traits;

[RegisterComponent]
public sealed partial class SkillchipImplantHolderComponent : Component
{
    // This component is just here to inform the system. Nothing else.

    // What, did you think there'd be a datafield here?
}

[RegisterComponent]
public sealed partial class SkilldeckComponent : Component
{
    [DataField]
    public int SkillchipSlotAmount = 3;
    public readonly string SkillchipSlotPrefix = "skillchip_";
}

[RegisterComponent]
public sealed partial class SkillchipComponent : Component
{
    /// <summary>
    ///     These functions are called when this skillchip is inside of a holder attached to an entity.
    /// </summary>
    [DataField(serverOnly: true)]
    public TraitFunction[] OnImplantFunctions { get; private set; } = Array.Empty<TraitFunction>();

    /// <summary>
    ///     These functions are called when this skillchip is inside of a holder removed from an entity.
    /// </summary>
    [DataField(serverOnly: true)]
    public TraitFunction[] OnRemoveFunctions { get; private set; } = Array.Empty<TraitFunction>();
}
