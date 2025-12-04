using System.Numerics;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Shuttles.BUIStates;
using Content.Shared.Shuttles.Components;

namespace Content.Server.Shuttles.Components
{
    [RegisterComponent]
    public sealed partial class ShuttleConsoleComponent : SharedShuttleConsoleComponent
    {
        [ViewVariables]
        public readonly List<EntityUid> SubscribedPilots = new();

        /// <summary>
        /// How much should the pilot's eye be zoomed by when piloting using this console?
        /// </summary>
        [DataField("zoom")]
        public Vector2 Zoom = new(1.5f, 1.5f);

        /// <summary>
        /// Should this console have access to restricted FTL destinations?
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite), DataField("whitelistSpecific")]
        public List<EntityUid> FTLWhitelist = new List<EntityUid>();

        /// <summary>
        ///     How far the shuttle is allowed to jump(in meters).
        ///     TODO: This technically won't work until this component is migrated to Shared. The client console screen will only ever know the hardcoded 512 meter constant otherwise. Fix it in ShuttleMapControl.xaml.cs when that's done.
        /// </summary>
        [DataField]
        public float FTLRange = 512f;

        /// <summary>
        ///     If the shuttle is allowed to "Forcibly" land on planet surfaces, destroying anything it lands on. Used for SSTO capable shuttles.
        /// </summary>
        [DataField]
        public bool FtlToPlanets;

        /// <summary>
        ///     If the shuttle is allowed to forcibly land on stations, smimshing everything it lands on. This is where the hypothetical "Nukie drop pod" comes into play.
        /// </summary>
        [DataField]
        public bool IgnoreExclusionZones;

        /// <summary>
        ///     If the shuttle is only ever allowed to FTL once. Also used for the hypothetical "Nukie drop pod."
        /// </summary>
        [DataField]
        public bool OneWayTrip;

        /// <summary>
        ///     Tracks whether or not the above "One way trip" has been taken.
        /// </summary>
        public bool OneWayTripTaken;

        /// Hullrot additions
        [DataField("targetIdSlot")]
        public ItemSlot targetIdSlot = new();

        // Hullrot - Auto Anchor 
        [DataField]
        public float AutoAnchorDelay = 30f;

        // Hullrot - Auto Anchor
        [ViewVariables]
        public float UnpowerAccumulated;

        // Hullrot - Auto Anchor
        [ViewVariables]
        public EntityUid? LastKnownGrid;

        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public ShuttleConsoleAccesState accesState = ShuttleConsoleAccesState.NotDynamic;

        // For dynamic ID indexing and shit.
        public string? captainIdentifier;
        public string? pilotIdentifier;
        public ShuttleBoundUserInterfaceState? LastUpdatedState = null;

        /// End hullrot additions
    }
}
