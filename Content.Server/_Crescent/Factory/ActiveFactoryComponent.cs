using Content.Shared.DeviceLinking;
using Robust.Shared.Prototypes;

namespace Content.Server.Factory.Components
{
    // Only used to query active & non-deleted factories
    [RegisterComponent]
    public sealed partial class ActiveFactoryComponent : Component
    {
    }
}
