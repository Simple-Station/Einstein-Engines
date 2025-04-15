using Content.Shared.DeviceLinking;
using Robust.Shared.Prototypes;

namespace Content.Server.Factory.Components
{
    // Only used to keep track of entities that are within the hitbox of the factory
    [RegisterComponent]
    public sealed partial class FactoryTrackingComponent : Component
    {
        public FactoryComponent FactoryReference;

        public EntityUid FactoryID;
    }
}
