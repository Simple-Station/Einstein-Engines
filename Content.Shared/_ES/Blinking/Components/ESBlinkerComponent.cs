using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._ES.Blinking.Components;

/// <summary>
/// Makes a character blink. That's it.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
[Access(typeof(ESSharedBlinkingSystem))]
public sealed partial class ESBlinkerComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan NextBlinkTime;

    [DataField]
    public TimeSpan MinBlinkDelay = TimeSpan.FromSeconds(2);

    [DataField]
    public TimeSpan MaxBlinkDelay = TimeSpan.FromSeconds(10);

    [DataField, AutoNetworkedField]
    public bool Enabled = true;
}

[Serializable, NetSerializable]
public enum ESBlinkVisuals : byte
{
    EyesClosed,
}
