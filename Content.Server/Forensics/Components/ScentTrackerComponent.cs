namespace Content.Server.Forensics
{
    [RegisterComponent]
    public sealed partial class ScentTrackerComponent : Component
    {
        /// <summary>
        /// The currently tracked scent.
        /// </summary>
        [DataField("scent")]
        public string Scent = String.Empty;

        /// <summary>
        /// The time (in seconds) that it takes to sniff an entity.
        /// </summary>
        [DataField("sniffDelay")]
        public float SniffDelay = 5.0f;
    }
}
