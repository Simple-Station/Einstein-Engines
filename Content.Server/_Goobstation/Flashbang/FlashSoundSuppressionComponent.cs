namespace Content.Server._Goobstation.Flashbang;

[RegisterComponent]
public sealed partial class FlashSoundSuppressionComponent : Component
{
    [DataField]
    public float ProtectionRange = 2f;
}
