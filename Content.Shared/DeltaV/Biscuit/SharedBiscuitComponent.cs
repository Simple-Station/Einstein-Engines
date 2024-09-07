using Robust.Shared.Serialization;

namespace Content.Shared.Biscuit;

public abstract partial class SharedBiscuitComponent : Component
{}

[Serializable, NetSerializable]
public enum BiscuitStatus : byte
{
    Cracked
}
