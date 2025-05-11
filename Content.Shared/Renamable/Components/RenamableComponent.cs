using Content.Shared.Renamable.EntitySystems;
using Robust.Shared.Serialization;


namespace Content.Shared.Renamable.Components;

[Serializable, NetSerializable]
public enum SharedRenamableInterfaceKey
{
    Key
}

[RegisterComponent, Access(typeof(SharedRenamableSystem))]
public sealed partial class RenamableComponent : Component
{
    /// <summary>
    /// Whether the entity can only be renamed once.
    /// If set to true, the component will be removed after confirming a name.
    /// </summary>
    [DataField]
    public bool SingleUse = false;
}
