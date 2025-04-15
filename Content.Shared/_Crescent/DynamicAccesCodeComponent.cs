using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Crescent;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class DynamicCodeHolderComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public HashSet<int> codes = new();

    [ViewVariables(VVAccess.ReadWrite)]
    public Dictionary<string, HashSet<int>> mappedCodes = new();

}

