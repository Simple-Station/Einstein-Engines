using Content.Shared.Actions;
using Content.Shared.Cybernetics;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Cybernetics
{
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
    public sealed partial class BluespaceShuntComponent : Component
    {
        /// <summary>
        ///    The action to add to the entity.
        ///   </summary>
        [DataField("actionproto"), AutoNetworkedField]
        public string ActionPrototype = "ActionBluespaceShunt";

        [DataField, AutoNetworkedField]
        public EntityUid? Action;

        [DataField, AutoNetworkedField]
        public bool OnCooldown = false;

        [DataField, AutoNetworkedField]
        public float CooldownTime = 5f;

    }
}
public sealed partial class BluespaceShuntEvent : WorldTargetActionEvent
{

}

public record struct BluespaceShuntUsedEvent(BluespaceShuntComponent Component)
{

};

public record struct BluespaceShuntCooldownEndEvent(BluespaceShuntComponent Component)
{

};


