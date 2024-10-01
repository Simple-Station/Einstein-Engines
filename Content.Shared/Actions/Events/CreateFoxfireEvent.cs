using Robust.Shared.Prototypes;

namespace Content.Shared.Actions.Events;

public sealed partial class CreateFoxfireActionEvent : InstantActionEvent
{
    /// <summary>
    ///     The foxfire prototype to use.
    /// </summary>
    [DataField]
    public EntProtoId FoxfirePrototype = "Foxfire";

    [DataField]
    public EntProtoId FoxfireActionId = "ActionFoxfire";

    public EntityUid? FoxfireAction;
}

public readonly record struct FoxFireDestroyedEvent;