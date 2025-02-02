using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.Silicons.Bots;

/// <summary>
/// todo: Make weldbots set targets on fire when emagged
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(WeldbotSystem))]
public sealed partial class EmaggableWeldbotComponent : Component
{
    /// <summary>
    /// Sound to play when the bot has been emagged
    /// </summary>
    [DataField]
    public SoundSpecifier SparkSound = new SoundCollectionSpecifier("sparks")
    {
        Params = AudioParams.Default.WithVolume(8f)
    };
}
