using Content.Shared.Actions;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Aliens.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class TailLashComponent : Component
{
    [DataField("tailLashAction", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string? TailLashAction = "ActionTailLash";

    [DataField("tailLashActionEntity")] public EntityUid? TailLashActionEntity;

    [DataField]
    public float LashRange = 2f;

    [DataField]
    public int StunTime = 5;

    [DataField]
    public int Cooldown = 11;

    [DataField("disarmSuccessSound")]
    public SoundSpecifier LashSound = new SoundPathSpecifier("/Audio/Effects/thudswoosh.ogg");
}

public sealed partial class TailLashActionEvent : InstantActionEvent { }
