using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Cybernetics
{
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
    public sealed partial class CortexBombComponent : Component
    {
        /// <summary>
        ///    The action to add to the entity.
        ///   </summary>
        [DataField("actionproto"), AutoNetworkedField]
        public string ActionPrototype = "ActionActivateCortexBomb";

        [DataField, AutoNetworkedField]
        public EntityUid? Action;
    }
}
public sealed partial class CortexBombActivatedEvent : InstantActionEvent
{

}
