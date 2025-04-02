using Content.Shared.Atmos;

namespace Content.Server.Atmos.Components
{
    /// <summary>
    /// Used by FixGridAtmos. Entities with this may get magically auto-deleted on map initialization in future.
    /// </summary>
    [RegisterComponent]
    public sealed partial class AtmosFixMarkerComponent : Component
    {
        // See FixGridAtmos for more details
        [DataField]
        public int Mode { get; set; } = 0;

        [DataField]
        public GasMixture? GasMix = default!;
    }
}
