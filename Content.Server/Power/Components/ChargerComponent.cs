using Content.Shared.Power;
using Content.Shared.Whitelist;

namespace Content.Server.Power.Components
{
    [RegisterComponent]
    public sealed partial class ChargerComponent : Component
    {
        [ViewVariables]
        public CellChargerStatus Status;

        /// <summary>
        /// The charge rate of the charger, in watts
        /// </summary>
        [DataField("chargeRate")]
        public float ChargeRate = 20.0f;

        /// <summary>
        /// The container ID that is holds the entities being charged.
        /// </summary>
        [DataField("slotId", required: true)]
        public string SlotId = string.Empty;

        /// <summary>
        /// The maximum number of batteries that can be charged by the recharge at once 
        /// </summary>
        [DataField]
        public int MaxBatteries = 1;

        /// <summary>
        /// A whitelist for what entities can be charged by this Charger.
        /// Most useful on chargers that can hold more than one battery, or chargers that use EntityStorage rather than a slot system.
        /// </summary>
        [DataField]
        public EntityWhitelist? ChargeWhitelist;

        /// <summary>
        /// A whitelist for what entities can be searched when looking for batteries to charge.
        /// Most useful on chargers that use EntityStorage rather than a slot system.
        /// 
        /// For instance, the cyborg recharging station can only search cyborgs or ipcs.
        /// Without it, a urist could stand in said recharging station to recharge inventory batteries.
        /// </summary>
        [DataField]
        public EntityWhitelist? SearchWhitelist;

        /// <summary>
        /// Indicates whether the charger is portable and thus subject to EMP effects
        /// and bypasses checks for transform, anchored, and ApcPowerReceiverComponent.
        /// </summary>
        [DataField]
        public bool Portable = false;

        /// <summary>
        /// debug
        /// </summary>
        [DataField]
        public List<EntityUid> ExpectedBatteries = new();

        /// <summary>
        /// Maximum number of search steps. In case of extremely complex nested inventories. While it should never occur, prevents recursion too.
        /// </summary>
        public int MaxSteps = 256;

        /// <summary>
        /// Maximum inventory depth. In case of extremely complex nested inventories. While it should never occur, prevents recursion too.
        /// </summary>
        public int MaxDepth = 10;
    }
}
