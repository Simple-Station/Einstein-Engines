using Robust.Shared.Audio;
using Robust.Shared.GameStates;
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
    ///   The number of charges left.
    /// </summary>
    [DataField]
    public int Charges = -1;

    /// <summary>
    ///   The maximum possible charges of self-extinguishes.
    /// </summary>
    [DataField(required: true)]
    public int MaxCharges;

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
