using Content.Shared.Containers.ItemSlots;
using Content.Shared.Shipyard;
using JetBrains.Annotations;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;


namespace Content.Shared.Shipyard
{
    // Note: when adding a new ui key, don't forget to modify the dictionary in SharedShipyardSystem
    [NetSerializable, Serializable]
    public enum ShipyardConsoleUiKey : byte
    {
        Key, // From EE's shipyard implem. Just here for compatibility's sake
        Shipyard,
        Security,
        Nfsd,
        Syndicate,
        BlackMarket,
        Expedition,
        Scrap,

        // Do not add any ship to this key. Shipyards using it are inherently empty and are populated using the ShipyardListingComponent.
        Custom
    }

    [NetSerializable, Serializable]
    public sealed class ShipyardConsoleInterfaceState : BoundUserInterfaceState
    {
        public int Balance;
        public readonly bool AccessGranted;
        public readonly string? ShipDeedTitle;
        public int ShipSellValue;
        public readonly bool IsTargetIdPresent;
        public readonly byte UiKey;

        public readonly List<string> ShipyardPrototypes;
        public readonly string ShipyardName;

        public ShipyardConsoleInterfaceState(
            int balance,
            bool accessGranted,
            string? shipDeedTitle,
            int shipSellValue,
            bool isTargetIdPresent,
            byte uiKey,
            List<string> shipyardPrototypes,
            string shipyardName)
        {
            Balance = balance;
            AccessGranted = accessGranted;
            ShipDeedTitle = shipDeedTitle;
            ShipSellValue = shipSellValue;
            IsTargetIdPresent = isTargetIdPresent;
            UiKey = uiKey;
            ShipyardPrototypes = shipyardPrototypes;
            ShipyardName = shipyardName;
        }
    }

    [Serializable, NetSerializable]
    public sealed class ShipyardConsoleSellMessage : BoundUserInterfaceMessage{}

    [RegisterComponent]
    public sealed partial class StationDeedSpawnerComponent : Component {}


    public abstract class SharedShipyardSystem : EntitySystem
    {
        /// <summary>
        ///   Maps entries of the <see cref="ShipyardConsoleUiKey"/> enum to how they're specified in shuttle prototype files
        /// </summary>
        public static readonly Dictionary<ShipyardConsoleUiKey, string> ShipyardGroupMapping = new()
        {
            { ShipyardConsoleUiKey.Shipyard, "Civilian" },
            { ShipyardConsoleUiKey.Security, "Security" },
            { ShipyardConsoleUiKey.Nfsd, "Nfsd" },
            { ShipyardConsoleUiKey.Syndicate, "Syndicate" },
            { ShipyardConsoleUiKey.BlackMarket, "BlackMarket" },
            { ShipyardConsoleUiKey.Expedition, "Expedition" },
            { ShipyardConsoleUiKey.Scrap, "Scrap" },
            { ShipyardConsoleUiKey.Custom, "<DO NOT USE>" }
        };

        [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<ShipyardConsoleComponent, ComponentInit>(OnComponentInit);
            SubscribeLocalEvent<ShipyardConsoleComponent, ComponentRemove>(OnComponentRemove);
        }

        private void OnComponentInit(EntityUid uid, ShipyardConsoleComponent component, ComponentInit args)
        {
            _itemSlotsSystem.AddItemSlot(uid, ShipyardConsoleComponent.TargetIdCardSlotId, component.TargetIdSlot);
        }

        private void OnComponentRemove(EntityUid uid, ShipyardConsoleComponent component, ComponentRemove args)
        {
            _itemSlotsSystem.RemoveItemSlot(uid, component.TargetIdSlot);
        }

        [Serializable, NetSerializable]
        private sealed class ShipyardConsoleComponentState : ComponentState
        {
            public List<string> AccessLevels;

            public ShipyardConsoleComponentState(List<string> accessLevels)
            {
                AccessLevels = accessLevels;
            }
        }

    }
}
