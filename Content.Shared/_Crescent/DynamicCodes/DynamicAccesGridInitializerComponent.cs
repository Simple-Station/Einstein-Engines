using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._Crescent.DynamicCodes;

/// <summary>
/// Uses the mapping to add the dynamic codes to each relevant entity along with the component needed
/// </summary>
[RegisterComponent]
public sealed partial class DynamicAccesGridInitializerComponent : Component
{
    [DataField("accesMapping"), ViewVariables(VVAccess.ReadWrite)]
    // this is the default initialization mapping for ships. If you change this make sure its valid SPCR 2025
    public string accesMapping = "GeneralShipAccesMapping";

}
