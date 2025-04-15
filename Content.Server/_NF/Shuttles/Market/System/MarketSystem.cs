using System.Linq;
using Content.Server._NF.Market.Components;
using Content.Server.Bank;
using Content.Server.Cargo.Systems;
using Content.Shared._NF.Market;
using Content.Shared._NF.Market.BUI;
using Content.Shared._NF.Market.Events;
using Content.Shared.Bank.Components;
using Content.Shared.Cargo.Components;
using Content.Shared.Materials;
using Content.Shared.Stacks;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server._NF.Market.Systems;

public sealed partial class MarketSystem : SharedMarketSystem
{
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    private readonly List<MarketData> _marketDataList = [];

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EntitySoldEvent>(OnEntitySoldEvent);
        SubscribeLocalEvent<MarketConsoleComponent, BoundUIOpenedEvent>(OnConsoleUiOpened);
        SubscribeLocalEvent<MarketConsoleComponent, CrateMachineCartMessage>(OnCartMessage);
        InitializeCrateMachine();
    }

    private void OnEntitySoldEvent(ref EntitySoldEvent ev)
    {
        foreach (var sold in ev.Sold)
        {
            // Get the MetaDataComponent from the sold entity
            if (_entityManager.TryGetComponent<MetaDataComponent>(sold, out var metaData))
            {
                // Get the prototype ID of the sold entity
                var entityPrototypeId = metaData.EntityPrototype?.ID;

                if (entityPrototypeId == null)
                    continue; // Skip items without prototype id

                var count = 1;
                TryComp<StackComponent>(sold, out var stackComponent);

                if (HasComp<MaterialComponent>(sold) && TryComp<PhysicalCompositionComponent>(sold, out var phys)) // special handling for mats
                {
                    var multiplier = stackComponent != null ? stackComponent.Count : 1; // handle material stacks

                    foreach (var (material, volume) in phys.MaterialComposition)
                    {
                        // If it's a valid material, store it
                        if (_prototypes.TryIndex<MaterialPrototype>(material, out var matProto))
                        {
                            TryUpdateMarketData('%' + material, MaterialVolumeToAmount(matProto, volume * multiplier), ev.Station);
                        }
                    }
                    return;
                }

                if (stackComponent != null)
                {
                    count = stackComponent.Count;
                }

                // Increase the count in the MarketData for this entity
                // Assuming the quantity to increase is 1 for each sold entity
                TryUpdateMarketData(entityPrototypeId, count, ev.Station);
            }
        }
    }

    private void OnCartMessage(EntityUid consoleUid, MarketConsoleComponent consoleComponent,
        ref CrateMachineCartMessage args)
    {
        if (args.Actor is not { Valid: true } player)
            return;
        if (!TryComp<BankAccountComponent>(player, out var bank))
            return;
        var marketMultiplier = 1.0f;
        if (TryComp<MarketModifierComponent>(consoleUid, out var priceMod))
        {
            marketMultiplier = priceMod.Mod;
        }

        var station = Transform(consoleUid).GridUid;
        if (station == null)
            return;

        if (TryUpdateMarketData(args.ItemPrototype, -args.Amount, station.Value))
        {
            var stationNetEntity = GetNetEntity(station.Value);
            var itemProto = args.ItemPrototype;
            // Find the MarketData for the given EntityPrototype
            var marketData = consoleComponent.CartData.FirstOrDefault(md => md.Prototype == itemProto && md.StationUid == stationNetEntity);
            if (marketData != null)
            {
                // If it exists, change the count
                marketData.Quantity += args.Amount;
                if (marketData.Quantity <= 0)
                {
                    consoleComponent.CartData.Remove(marketData);
                }
            }
            else if (args.Amount > 0)
            {
                consoleComponent.CartData.Add(new MarketData(args.ItemPrototype, args.Amount, stationNetEntity));
            }
        }
        RefreshState(consoleUid, bank.Balance, marketMultiplier, _marketDataList, consoleComponent.CartData, MarketConsoleUiKey.Default);
    }


    /// <summary>
    /// Updates the market data list or adds it new if it doesnt exist in there yet.
    /// </summary>
    /// <param name="entityPrototypeId"></param>
    /// <param name="increaseAmount"></param>
    public bool TryUpdateMarketData(string entityPrototypeId, int increaseAmount, EntityUid station)
    {
        var stationNetEntity = GetNetEntity(station);
        // Find the MarketData for the given EntityPrototype
        var marketData = _marketDataList.FirstOrDefault(md => md.Prototype == entityPrototypeId && md.StationUid == stationNetEntity);

        if (marketData != null)
        {
            // If it exists, change the count
            marketData.Quantity += increaseAmount;
            if (marketData.Quantity <= 0)
            {
                _marketDataList.Remove(marketData);
            }
            return true;
        }
        // If it doesn't exist, create a new MarketData and add it to the list
        if (increaseAmount > 0)
        {
            _marketDataList.Add(new MarketData(entityPrototypeId, increaseAmount, stationNetEntity));
            return true;
        }
        return false;
    }

    private void OnConsoleUiOpened(EntityUid uid, MarketConsoleComponent component, BoundUIOpenedEvent args)
    {
        if (args.Actor is not { Valid: true } player)
            return;
        if (!TryComp<BankAccountComponent>(player, out var bank))
            return;
        var marketMultiplier = 1.0f;
        if (TryComp<MarketModifierComponent>(uid, out var priceMod))
        {
            marketMultiplier = priceMod.Mod;
        }

        RefreshState(uid, bank.Balance, marketMultiplier, _marketDataList, component.CartData, MarketConsoleUiKey.Default);
    }

    private int GetMarketSelectionValue(List<MarketData> dataList, float marketModifier)
    {
        var cartBalance = 0;

        if (!(dataList.Count >= 1))
            return cartBalance;

        foreach (var marketData in dataList)
        {
            cartBalance += GetEntryPrice(marketData.Prototype, marketModifier) * marketData.Quantity;
        }
        return cartBalance;
    }

    private int GetMarketSelectionValue(List<string> dataList, float marketModifier)
    {
        var cartBalance = 0;

        if (dataList.Count <= 0)
            return cartBalance;

        foreach (var name in dataList)
        {
            cartBalance += GetEntryPrice(name, marketModifier);
        }

        return cartBalance;
    }


    private int GetEntryPrice(string entry, float marketModifier)
    {
        if (entry[0] == '%' && _prototypeManager.TryIndex<MaterialPrototype>(entry[1..], out var material))
        {
            return (int) Math.Round(material.Price * StandardMaterialAmount(material));
        }

        if (!_prototypeManager.TryIndex<EntityPrototype>(entry, out var prototype))
        {
            return 0;
        }

        var price = 0f;

        // always respect static price for entities
        if (prototype.TryGetComponent<StaticPriceComponent>(out var staticPrice))
        {
            price = (float) (staticPrice.Price * marketModifier);
        }

        return (int) Math.Round(price);
    }



    private void RefreshState(
        EntityUid uid,
        int balance,
        float marketMultiplier,
        List<MarketData> data,
        List<MarketData> cartData,
        MarketConsoleUiKey uiKey
    )
    {
        var cartBalance = GetMarketSelectionValue(cartData, marketMultiplier);

        var newState = new MarketConsoleInterfaceState(
            balance,
            marketMultiplier,
            data,
            cartData,
            cartBalance,
            true // TODO add enable/disable functionality
        );
        _ui.SetUiState(uid, uiKey, newState);
    }
}
