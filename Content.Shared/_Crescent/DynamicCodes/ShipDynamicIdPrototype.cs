using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Set;

namespace Content.Shared._Crescent.DynamicCodes;

/// <summary>
/// This is a prototype for...
/// </summary>
[Prototype("shipDynamicAccesMapping")]
public sealed partial class ShipDynamicAccesMappingPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; } = default!;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public Dictionary<string, HashSet<string>> accesIdentifierToEntity = default!;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    // this key will be considered the captain key for the shuttle consoles on the grid
    public string captainKey = "Captain";

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    // this key will be considered the pilot key for the shuttle consoles on the grid
    public string pilotKey = "Pilot";

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public HashSet<string> CryoKeys = new (){"Crew"};
}
