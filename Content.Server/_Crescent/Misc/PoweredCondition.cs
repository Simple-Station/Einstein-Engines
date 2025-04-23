using Content.Server.Power.Components;

namespace Content.Server.NPC.HTN.Preconditions
{
    /// <summary>
    /// Returns true if the entity has an ApcPowerReceiver component in Powered state
    /// </summary>
    public sealed partial class PoweredPrecondition : HTNPrecondition
    {
        [Dependency] private readonly IEntityManager _entManager = default!;

        [DataField("poweredIfNoPowerReceiver")]
        public bool PoweredIfNoPowerReceiver = false;

        public override bool IsMet(NPCBlackboard blackboard)
        {
            if (blackboard.TryGetValue<EntityUid>(NPCBlackboard.Owner, out var owner, _entManager))
            {
                if (_entManager.TryGetComponent<ApcPowerReceiverComponent>(owner, out var apcPowerReceiver))
                {
                    return apcPowerReceiver.Powered;
                }

                return PoweredIfNoPowerReceiver;
            }

            return false;
        }
    }
}
