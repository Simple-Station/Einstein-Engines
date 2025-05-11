using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Content.Shared.Physics;
using Content.Shared.Projectiles;
using Robust.Shared.Map;
using Robust.Shared.Maths;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;


namespace Content.Shared._Crescent;

/// <summary>
/// Prevents projectile from phasing at ultra-high speeds through objects. Expected to be the only source of collisions (so remove all hard fixtures from any entity it is applied to)
///
/// </summary>
[RegisterComponent]
public sealed partial class ProjectilePhasePreventComponent : Component
{
    public Vector2 start = Vector2.Zero;
    public MapId mapId = MapId.Nullspace;
    public object containedAt;
    [ViewVariables(VVAccess.ReadWrite), DataField("mask", customTypeSerializer: typeof(FlagSerializer<CollisionMask>))]
    public int relevantBitmasks = 0;

}
