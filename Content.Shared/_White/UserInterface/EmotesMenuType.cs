using Robust.Shared.Serialization;

namespace Content.Shared._White.UserInterface;

[Serializable, NetSerializable]
public enum EmotesMenuType
{
    /// <summary>
    ///     The classic White Dream emotes menu
    /// </summary>
    Window,
    /// <summary>
    ///     The temporary SS14 emotes menu
    /// </summary>
    Radial,
}
