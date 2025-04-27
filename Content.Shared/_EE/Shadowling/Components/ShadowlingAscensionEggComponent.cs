using Robust.Shared.GameStates;


namespace Content.Shared._EE.Shadowling.Components;


/// <summary>
/// This is used for the Ascension Egg
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingAscensionEggComponent : Component
{
    [DataField]
    public TimeSpan NextUpdateTime = TimeSpan.Zero;

    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(300);

    [DataField]
    public EntityUid? Creator;

    [DataField]
    public bool StartTimer;

    [DataField]
    public string VerbName = "Start Ascension";
}
