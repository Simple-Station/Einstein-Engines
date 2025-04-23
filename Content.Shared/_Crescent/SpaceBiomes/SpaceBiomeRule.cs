using Content.Shared.Access;
using Content.Shared.Maps;
using Content.Shared.Roles;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Shared.Random;

public sealed partial class InSpaceBiomeRule : RulesRule
{
    [DataField]
    public string Biome;
}
