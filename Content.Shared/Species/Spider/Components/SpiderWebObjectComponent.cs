using Content.Shared.Species.Spider.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.Species.Spider.Components;

[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedSpiderSystem))]
public sealed partial class SpiderWebObjectComponent : Component
{
}
