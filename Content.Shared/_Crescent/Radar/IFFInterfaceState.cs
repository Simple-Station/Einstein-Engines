using Robust.Shared.Serialization;

namespace Content.Shared.Crescent.Radar;

[Serializable, NetSerializable]
public sealed class IFFInterfaceState
{
    public List<ProjectileState> Projectiles;
    public Dictionary<NetEntity, List<TurretState>> Turrets;

    public IFFInterfaceState(List<ProjectileState> projectiles, Dictionary<NetEntity, List<TurretState>> turrets)
    {
        Projectiles = projectiles;
        Turrets = turrets;
    }
}
