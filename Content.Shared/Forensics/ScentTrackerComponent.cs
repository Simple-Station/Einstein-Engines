using Robust.Shared.GameStates;

namespace Content.Shared.Forensics
{
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
    public sealed partial class ScentTrackerComponent : Component
    {
        /// <summary>
        ///     The currently tracked scent.
        /// </summary>
        [DataField, AutoNetworkedField]
        public string Scent = String.Empty;

        /// <summary>
        ///     The time (in seconds) that it takes to sniff an entity.
        /// </summary>
        [DataField]
        public float SniffDelay = 5.0f;
    }
}
