using Content.Shared.Access.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Set;
using Robust.Shared.Serialization;

namespace Content.Shared.NamedModules.Components;

[RegisterComponent, NetworkedComponent,AutoGenerateComponentState]
public sealed partial class NamedModulesComponent : Component
{
    [AutoNetworkedField, DataField("buttonNames"), ViewVariables(VVAccess.ReadWrite)]
    public List<string> ButtonNames = new();

}

[NetSerializable, Serializable]
public sealed class ModuleNamingChangeEvent : BoundUserInterfaceMessage
{

    public readonly List<string> NewNames;

    public ModuleNamingChangeEvent(List<string> names)
    {

        NewNames = names;
    }
}

