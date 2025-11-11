namespace Content.Server.Medical.Components
{
    /// <summary>
    ///  This is the buffer, that impedes from going into crit.
    /// </summary>
    [RegisterComponent]
    public sealed partial class CritThresholdBufferComponent : Component
    {
        [DataField("bufferHp")] public float BufferHp = 0f;
        [DataField("expiresAt")] public TimeSpan ExpiresAt;
        [DataField("showPopup")] public bool ShowPopup = true;
    }
}
