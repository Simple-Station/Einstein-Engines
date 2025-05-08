using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Content.Shared.Projectiles;
using Robust.Shared.Map;
using Robust.Shared.Maths;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._Crescent;

/// <summary>
/// This handles...
///
/// </summary>
[RegisterComponent]
public sealed partial class ProjectilePhasePreventComponent : Component
{
    public HashSet<EntityUid> ignoredEntities = new HashSet<EntityUid>();
    public Vector2 start = Vector2.Zero;
    public Vector2 translation = Vector2.Zero;
    public MapId mapId = MapId.Nullspace;
    public int containedAt = 0;
    public int relevantBitmasks = 0;

}
