using Content.Shared.Damage.Prototypes;
using Content.Shared.StepTrigger.Components;
using Content.Shared.StepTrigger.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Shared.StepTrigger.Prototypes
{
    /// <summary>
    /// A group of <see cref="StepTriggerTypePrototype">
    /// Used to determine StepTriggerTypes like Tags.
    /// Used for better work with Immunity.
    /// StepTriggerTypes in StepTriggerTypes.yml
    /// WD EDIT
    /// </summary>
    /// <code>
    /// stepTriggerGroups:
    ///   types:
    ///   - Lava
    ///   - Landmine
    ///   - Shard
    ///   - Chasm
    ///   - Mousetrap
    ///   - SlipTile
    ///   - SlipEntity
    /// </code>
    [DataDefinition]
    [Serializable, NetSerializable]
    public sealed partial class StepTriggerGroup
    {
        [DataField]
        public List<ProtoId<StepTriggerTypePrototype>>? Types = null;

        /// <summary>
        /// Checks if types of this StepTriggerGroup is similar to types of AnotherGroup
        /// </summary>
        public bool IsValid(StepTriggerGroup? AnotherGroup)
        {
            if (Types != null)
            {
                foreach (var type in Types)
                {
                    if (AnotherGroup != null
                        && AnotherGroup.Types != null
                        && AnotherGroup.Types.Contains(type))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks validation (if types of this StepTriggerGroup are similar to types of
        /// another StepTriggerComponent.
        /// </summary>
        public bool IsValid(StepTriggerComponent component)
        {
            if (component.TriggerGroups != null)
            {
                return IsValid(component.TriggerGroups);
            }
            return false;
        }

        /// <summary>
        /// Checks validation (if types of this StepTriggerGroup are similar to types of
        /// another StepTriggerImmuneComponent.
        /// </summary>
        public bool IsValid(StepTriggerImmuneComponent component)
        {
            if (component.Whitelist != null)
                return IsValid(component.Whitelist);
            return false;
        }
    }
}
