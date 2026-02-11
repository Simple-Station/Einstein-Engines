using Robust.Shared.Serialization;
namespace Content.Shared._White.Bark;

[Serializable, NetSerializable]
public sealed class EntityBarkEvent(NetEntity entity, List<BarkData> barks) : EntityEventArgs
{
    public NetEntity Entity { get; } = entity;
    public List<BarkData> Barks { get; } = barks;
}
