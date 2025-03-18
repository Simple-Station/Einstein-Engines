using Content.Server.StationEvents.Events;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.StationEvents.Components;

/// <summary>
/// Make everyone speak in tongues.
/// (Cogchamp's accent)
/// </summary>
[RegisterComponent, Access(typeof(BabelTowerRule))]
public sealed partial class BabelTowerRuleComponent : Component { }
