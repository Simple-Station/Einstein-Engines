using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Prototypes;
using Content.Shared.Actions;
using Robust.Shared.Utility;

namespace Content.Server.Abilities.Kitsune;

[RegisterComponent]
public sealed partial class KitsuneComponent : Component
{
    /// <summary>
    /// The foxfire prototype to use.
    /// </summary>
    [DataField("foxfirePrototype")]
    public EntProtoId FoxfirePrototype = "Foxfire";

    [DataField]
    public EntProtoId FoxfireActionId = "ActionFoxfire";

    public EntityUid? FoxfireAction;
}

