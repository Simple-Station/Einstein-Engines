// <summary>
//   Locks an item to only be used in melee by entities with a specific component.
// </summary>

using Robust.Shared.Audio;
using Robust.Shared.Prototypes;


[RegisterComponent]
public sealed partial class RestrictedMeleeComponent : Component
{
    [DataField, AlwaysPushInheritance]
    public ComponentRegistry AllowedComponents { get; private set; } = new();

    [DataField]
    public TimeSpan KnockdownDuration = TimeSpan.FromSeconds(2);

    [DataField]
    public string FailText { get; set; } = "oni-only-component-fail-self";

    [DataField]
    public bool DoKnockdown = true;

    [DataField]
    public bool ForceDrop = true;

    [DataField]
    public SoundSpecifier FallSound = new SoundPathSpecifier("/Audio/Effects/slip.ogg");
}
