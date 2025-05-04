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

}
