using System.Threading.Tasks;
using Content.Server._NF.Market.Components;
using Content.Server.Bank;
using Content.Server.Storage.Components;
using Content.Shared._NF.Market;
using Content.Shared._NF.Market.Components;
using Content.Shared._NF.Market.Events;
using Content.Shared.Bank.Components;
using Content.Shared.Maps;
using Content.Shared.Storage.EntitySystems;
using Content.Shared.Materials;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using static Content.Shared._NF.Market.Components.SharedCrateMachineComponent;

namespace Content.Server._NF.Market.Systems;

public sealed partial class MarketSystem
{
    [Dependency] private readonly AppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly BankSystem _bankSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly SharedEntityStorageSystem _entityStorage = default!;


    private const int MaxCrateMachineDistance = 16;

    private void InitializeCrateMachine()
    {
        SubscribeLocalEvent<MarketConsoleComponent, CrateMachinePurchaseMessage>(OnMarketConsolePurchaseCrateMessage);
    }

    /// <summary>
    /// Calculates distance between two EntityCoordinates
    /// Used to check for cargo pallets around the console instead of on the grid.
    /// </summary>
    /// <param name="point1">first point to get distance between</param>
    /// <param name="point2">second point to get distance between</param>
    /// <returns></returns>
    public static double CalculateDistance(EntityCoordinates point1, EntityCoordinates point2)
    {
        var xDifference = point2.X - point1.X;
        var yDifference = point2.Y - point1.Y;

        return Math.Sqrt(xDifference * xDifference + yDifference * yDifference);
    }

    private void OnMarketConsolePurchaseCrateMessage(EntityUid consoleUid, MarketConsoleComponent component, ref CrateMachinePurchaseMessage args)
    {
        var crateMachineQuery = AllEntityQuery<CrateMachineComponent, TransformComponent>();

        var marketMod = 1f;
        if (TryComp<MarketModifierComponent>(consoleUid, out var marketModComponent))
        {
            marketMod = marketModComponent.Mod;
        }

        while (crateMachineQuery.MoveNext(out var crateMachineUid, out var comp, out var compXform))
        {
            var distance = CalculateDistance(compXform.Coordinates, Transform(consoleUid).Coordinates);
            var maxCrateMachineDistance = MaxCrateMachineDistance;

            // Get the mapped checking distance from the console
            if (TryComp<MarketConsoleComponent>(consoleUid, out var cargoShuttleComponent))
            {
                maxCrateMachineDistance = cargoShuttleComponent.MaxCrateMachineDistance;
            }
            var isTooFarAway = distance > maxCrateMachineDistance;

            if (!compXform.Anchored || isTooFarAway)
            {
                continue;
            }

            // We found the first nearby compatible crate machine.
            OnPurchaseCrateMessage(crateMachineUid, comp, component, marketMod, args);

            break;
        }

        TryComp<BankAccountComponent>(args.Actor, out var bank);
        RefreshState(consoleUid, bank?.Balance ?? 0, marketMod, _marketDataList, component.CartData, MarketConsoleUiKey.Default);
    }

    private void OnPurchaseCrateMessage(EntityUid crateMachineUid, SharedCrateMachineComponent component, MarketConsoleComponent consoleComponent, float marketMod, CrateMachinePurchaseMessage args)
    {

        if (args.Actor is not { Valid: true } player)
            return;

        if (!TryComp<BankAccountComponent>(player, out var bankAccount))
            return;

        TrySpawnCrate(crateMachineUid, player, component, consoleComponent, marketMod, bankAccount);
    }

    private bool isAnimationRunning = false;
    public void TrySpawnCrate(EntityUid crateMachineUid, EntityUid player, SharedCrateMachineComponent component, MarketConsoleComponent consoleComponent, float marketMod, BankAccountComponent playerBank)
    {
        if (isAnimationRunning)
            return;

        if (consoleComponent.CartData.Count <= 0)
            return;

        if (!TryComp<TransformComponent>(crateMachineUid, out var crateMachineTransform) ||
            !TryComp<MapGridComponent>(crateMachineTransform.GridUid, out var grid))
            return;

        var tileRef = crateMachineTransform.Coordinates.GetTileRef(EntityManager, _mapManager);
        if (tileRef == null)
        {
            return;
        }

        foreach (var entity in _lookup.GetLocalEntitiesIntersecting(tileRef.Value, flags: LookupFlags.All | LookupFlags.Approximate))
        {
            if (_entityManager.GetComponent<MetaDataComponent>(entity).EntityPrototype?.ID == component.CratePrototype)
            {
                // Don't spawn a crate if theres already one on the platform.
                return;
            }
        }

        var cartBalance = GetMarketSelectionValue(consoleComponent.CartData, marketMod);
        if (!(playerBank.Balance >= cartBalance))
            return;

        var spawnList = new List<string>();
        int i = 0;
        while (i++ < 30 && consoleComponent.CartData.Count > 0)
        {
            var marketData = consoleComponent.CartData[0];
            if (marketData.Quantity > 0)
            {
                marketData.Quantity -= 1;
                spawnList.Add(marketData.Prototype);
            }

            if (marketData.Quantity <= 0)
            {
                consoleComponent.CartData.RemoveAt(0);
            }
        }

        // Withdraw spesos from player
        //_bankSystem.TryBankWithdraw(player, );
        var spawnCost = GetMarketSelectionValue(spawnList, marketMod);
        if (!_bankSystem.TryBankWithdraw(player, spawnCost))
            return;

        var xform = Transform(crateMachineUid);

        isAnimationRunning = true;

        // This task has no exception tolerance and should probably be rewritten -Rane
        Task.Run(async () =>
        {
            UpdateVisualState(crateMachineUid, component, true);
            Dirty(crateMachineUid, component);
            await Task.Delay(3000);
            UpdateVisualState(crateMachineUid, component, true);
            Dirty(crateMachineUid, component);
            await Task.Delay(1000);
            var targetCrate = Spawn(component.CratePrototype, xform.Coordinates);
            UpdateVisualState(crateMachineUid, component, false);
            Dirty(crateMachineUid, component);
            isAnimationRunning = false;
            SpawnCrateItems(spawnList, targetCrate);
        });
    }

    // This doesn't adding stacks but I CBA rewriting it now. Also the duplicated spawn calls are a waste -Rane
    // Fixing that should also fix non-material stacks like cables
    private void SpawnCrateItems(List<string> spawnList, EntityUid targetCrate)
    {
        if (TryComp<EntityStorageComponent>(targetCrate, out var entityStorage))
        {
            _entityStorage.OpenStorage(targetCrate, entityStorage);

            foreach (var prototype in spawnList)
            {
                var spawnedProto = prototype;

                if (prototype[0] == '%' && _prototypeManager.TryIndex<MaterialPrototype>(prototype[1..], out var material))
                {
                    spawnedProto = material.StackEntity;
                }

                var spawn = Spawn(spawnedProto, Transform(targetCrate).Coordinates);
                _entityStorage.Insert(targetCrate, spawn, entityStorage);
            }

            _entityStorage.CloseStorage(targetCrate, entityStorage);
        }
    }

    public void UpdateVisualState(EntityUid uid, SharedCrateMachineComponent component, bool isOpening = true)
    {
        if (!TryComp(uid, out AppearanceComponent? appearance))
        {
            return;
        }

        // Unanchored should not animate.
        if (!Transform(uid).Anchored)
        {
            return;
        }

        if (isOpening)
        {
            _appearanceSystem.SetData(uid, CrateMachineVisuals.VisualState, CrateMachineVisualState.Opening, appearance);
        }
        else
        {
            _appearanceSystem.SetData(uid, CrateMachineVisuals.VisualState, CrateMachineVisualState.Closing, appearance);
        }
    }

}
