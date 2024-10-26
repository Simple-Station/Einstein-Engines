using Content.Server.Body.Systems;
using Robust.Shared.GameStates;

namespace Content.Server.Body.Components
{
    [RegisterComponent, Access(typeof(BrainSystem))]
    public sealed partial class BrainComponent : Component
    {
    }
}
