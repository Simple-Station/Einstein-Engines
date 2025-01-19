using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.Extinguisher;

/// <summary>
///   When equipped, the SelfExtinguisherComponent will try to automatically extinguish its wearer when on fire.
/// </summary>
[RegisterComponent]
public partial class SelfExtinguisherComponent : Component
{
    /// <summary>
    ///     Action used to self-extinguish.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntProtoId Action = "ActionSelfExtinguish";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity;

    /// <summary>
    ///   Cooldown before the self-extinguisher can be used again.
    /// </summary>
    [DataField(required: true)]
    public TimeSpan Cooldown;

    /// <summary>
    ///   Time before the self-extinguisher can be used again.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextExtinguish = TimeSpan.Zero;

    /// <summary>
    ///   If true, requires the wearer to be immune to gas ignition.
    /// </summary>
    [DataField]
    public bool RequiresIgniteFromGasImmune = false;

    /// <summary>
    ///   The sound effect that plays upon an extinguish.
    /// </summary>
    [DataField(required: true)]
    public SoundSpecifier Sound { get; private set; } = default!;
}
