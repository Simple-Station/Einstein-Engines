using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared.DeltaV.CCVars;
using Content.Shared.Tag;
using Robust.Server.GameObjects;
using Robust.Shared.Configuration;
using Content.Server.Shuttles.Systems;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles;
using Content.Server.Station.Components;
using Content.Server.Cargo.Systems;
using Content.Server.Station.Systems;
using Content.Shared.Shipyard;
using Content.Shared.Shipyard;
using Content.Shared.GameTicking;
using Content.Shared.Interaction;
using Robust.Server.GameObjects;
using Content.Shared.Mobs.Components;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.EntitySerialization;
using Robust.Shared.Map;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using Content.Server._Crescent;
using Content.Shared._Crescent;
using Robust.Shared.Containers;
using Robust.Shared.Map.Components;
using Content.Server._Crescent.DynamicAcces;
using Content.Server.Access.Systems;
using Content.Server.Administration.Logs;
using Content.Server.Bank;
using Content.Server.Chat.Systems;
using Content.Server.Maps;
using Content.Server.Mind;
using Content.Server.Popups;
using Content.Server.Preferences.Managers;
using Content.Server.Radio.EntitySystems;
using Content.Shared._Crescent.DynamicCodes;
using Content.Shared._Crescent.Helpers;
using Content.Shared._Crescent.ShipBalanceEnforcement;
using Content.Shared.Access;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Bank.Components;
using Content.Shared.Chat;
using Content.Shared.Crescent.Vouchers;
using Content.Shared.Database;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Mobs.Systems;
using Content.Shared.Radio;
using Content.Shared.Shipyard.Components;
using Content.Shared.Shipyard.Prototypes;
using Content.Shared.Shuttles.Components;
using Content.Shared.UserInterface;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Server.Shipyard;

public sealed partial class ShipyardSystem : SharedShipyardSystem
{
    [Dependency] private readonly IConfigurationManager _configManager = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly CargoSystem _cargo = default!;
    [Dependency] private readonly DockingSystem _docking = default!;
    [Dependency] private readonly PricingSystem _pricing = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedMapSystem _mapping = default!;
    [Dependency] private readonly DynamicCodeSystem _gridAcces = default!;
    [Dependency] private readonly AccessSystem _accessSystem = default!;
    [Dependency] private readonly AccessReaderSystem _access = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly IServerPreferencesManager _prefManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly RadioSystem _radio = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly BankSystem _bank = default!;
    [Dependency] private readonly IdCardSystem _idSystem = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MetaDataSystem _metadata = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly CrescentHelperSystem _crescent = default!;
    [Dependency] private readonly DynamicCodeSystem _codes = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelists = default!;

    public MapId? ShipyardMap { get; private set; }
    private float _shuttleIndex;
    private const float ShuttleSpawnBuffer = 1f;
    private ISawmill _sawmill = default!;
    private bool _enabled = true;



    public void HullrotInitialize()
    {
        _sawmill = Logger.GetSawmill("shipyard");

        SubscribeLocalEvent<ShipyardConsoleComponent, ComponentStartup>(OnShipyardStartup);
        SubscribeLocalEvent<ShipyardConsoleComponent, BoundUIOpenedEvent>(OnConsoleUIOpened);
        SubscribeLocalEvent<ShipyardConsoleComponent, ShipyardConsoleSellMessage>(OnSellMessage);
        SubscribeLocalEvent<ShipyardConsoleComponent, ShipyardConsolePurchaseMessage>(OnPurchaseMessage);
        SubscribeLocalEvent<ShipyardConsoleComponent, EntInsertedIntoContainerMessage>(OnItemSlotChanged);
        SubscribeLocalEvent<ShipyardConsoleComponent, EntRemovedFromContainerMessage>(OnItemSlotChanged);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
        SubscribeLocalEvent<StationDeedSpawnerComponent, MapInitEvent>(OnInitDeedSpawner);
        SubscribeLocalEvent<ShipyardConsoleComponent, InteractUsingEvent>(OnInteractUsing);
    }

    private void OnShipyardStartup(EntityUid uid, ShipyardConsoleComponent component, ComponentStartup args)
    {
        if (!_enabled)
            return;
        SetupShipyard();
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        CleanupShipyard();
    }

    private void SetShipyardEnabled(bool value)
    {
        if (_enabled == value)
            return;

        _enabled = value;

        if (value)
        {
            SetupShipyard();
        }
        else
        {
            CleanupShipyard();
        }
    }

    /// <summary>
    /// Adds a ship to the shipyard, calculates its price, and attempts to ftl-dock it to the given station
    /// </summary>
    /// <param name="stationUid">The ID of the station to dock the shuttle to</param>
    /// <param name="shuttlePath">The path to the shuttle file to load. Must be a grid file!</param>
    public bool TryPurchaseShuttle(EntityUid stationUid, string shuttlePath, [NotNullWhen(true)] out ShuttleComponent? shuttle, out DockingConfig? config)
    {
        config = null;
        if (!TryComp<StationDataComponent>(stationUid, out var stationData) || !TryAddShuttle(shuttlePath, out var shuttleGrid) || !TryComp<ShuttleComponent>(shuttleGrid, out shuttle))
        {
            shuttle = null;
            return false;
        }

        var price = _pricing.AppraiseGrid((EntityUid) shuttleGrid, null);
        var targetGrid = _station.GetLargestGrid(stationData);

        if (targetGrid == null) //how are we even here with no station grid
        {
            Del(shuttleGrid);
            shuttle = null;
            return false;
        }

        _sawmill.Info($"Shuttle {shuttlePath} was purchased at {ToPrettyString((EntityUid) stationUid)} for {price:f2}");
        //can do TryFTLDock later instead if we need to keep the shipyard map paused
        _shuttle.TryFTLDock( shuttleGrid.Value, shuttle, targetGrid.Value);

        return true;
    }

    /// <summary>
    /// Loads a shuttle into the ShipyardMap from a file path
    /// </summary>
    /// <param name="shuttlePath">The path to the grid file to load. Must be a grid file!</param>
    /// <returns>Returns the EntityUid of the shuttle</returns>
    private bool TryAddShuttle(string shuttlePath, [NotNullWhen(true)] out EntityUid? shuttleGrid)
    {
        shuttleGrid = null;
        if (ShipyardMap == null)
            return false;

        if (!_mapLoader.TryLoadGrid(ShipyardMap.Value, new ResPath(shuttlePath), out var grid, null, new Vector2(500f + _shuttleIndex, 1f)))
        {
            _sawmill.Error($"Unable to spawn shuttle {shuttlePath}");
            return false;
        }
        ;

        _shuttleIndex += _mapManager.GetAllMapGrids(ShipyardMap.Value).First().LocalAABB.Width + ShuttleSpawnBuffer;

        shuttleGrid = grid.Value.Owner;
        return true;
    }

    /// <summary>
    /// Checks a shuttle to make sure that it is docked to the given station, and that there are no lifeforms aboard. Then it appraises the grid, outputs to the server log, and deletes the grid
    /// </summary>
    /// <param name="stationUid">The ID of the station that the shuttle is docked to</param>
    /// <param name="shuttleUid">The grid ID of the shuttle to be appraised and sold</param>
    public bool TrySellShuttle(EntityUid stationUid, EntityUid shuttleUid, out int bill)
    {
        bill = 0;

        if (!TryComp<StationDataComponent>(stationUid, out var stationGrid) || !HasComp<ShuttleComponent>(shuttleUid) || !TryComp<TransformComponent>(shuttleUid, out var xform) || ShipyardMap == null)
            return false;

        var targetGrid = _station.GetLargestGrid(stationGrid);

        if (targetGrid == null)
            return false;

        var gridDocks = _docking.GetDocks((EntityUid) targetGrid);
        var shuttleDocks = _docking.GetDocks(shuttleUid);
        var isDocked = false;

        foreach (var shuttleDock in shuttleDocks)
        {
            foreach (var gridDock in gridDocks)
            {
                if (shuttleDock.Comp.DockedWith == gridDock.Owner)
                {
                    isDocked = true;
                    break;
                }
            }
            if (isDocked)
                break;
        }

        if (!isDocked)
        {
            _sawmill.Warning($"shuttle is not docked to that station");
            return false;
        }

        var mobQuery = GetEntityQuery<MobStateComponent>();
        var xformQuery = GetEntityQuery<TransformComponent>();

        if (FoundOrganics(shuttleUid, mobQuery, xformQuery))
        {
            _sawmill.Warning($"organics on board");
            return false;
        }

        //just yeet and delete for now. Might want to split it into another function later to send back to the shipyard map first to pause for something
        //also superman 3 moment
        if (_station.GetOwningStation(shuttleUid) is { Valid : true } shuttleStationUid)
        {
            _station.DeleteStation(shuttleStationUid);
        }

        bill = (int) _pricing.AppraiseGrid(shuttleUid);
        _mapManager.DeleteGrid(shuttleUid);
        _sawmill.Info($"Sold shuttle {shuttleUid} for {bill}");
        return true;
    }

    private void CleanupShipyard()
    {
        if (ShipyardMap == null || !_mapManager.MapExists(ShipyardMap.Value))
        {
            ShipyardMap = null;
            return;
        }

        _mapManager.DeleteMap(ShipyardMap.Value);
    }

    private void SetupShipyard()
    {
        if (ShipyardMap != null && _mapManager.MapExists(ShipyardMap.Value))
            return;
        _mapSystem.CreateMap(out var id);
        ShipyardMap = id;

        _mapManager.SetMapPaused(ShipyardMap.Value, false);
    }

    // <summary>
    // Tries to rename a shuttle deed and update the respective components.
    // Returns true if successful.
    //
    // Null name parts are promptly ignored.
    // </summary>
    public bool TryRenameShuttle(EntityUid uid, ShuttleDeedComponent? shuttleDeed,  string? newName, string? newSuffix)
    {
        if (!Resolve(uid, ref shuttleDeed))
            return false;

        var shuttle = shuttleDeed.ShuttleUid;
        if (shuttle != null
             && _station.GetOwningStation(shuttle.Value) is { Valid : true } shuttleStation)
        {
            shuttleDeed.ShuttleName = newName;
            shuttleDeed.ShuttleNameSuffix = newSuffix;
            Dirty(uid, shuttleDeed);

            var fullName = GetFullName(shuttleDeed);
            _station.RenameStation(shuttleStation, fullName, loud: false);
            _metaData.SetEntityName(shuttle.Value, fullName);
            _metaData.SetEntityName(shuttleStation, fullName);
        }
        else
        {
            _sawmill.Error($"Could not rename shuttle {ToPrettyString(shuttle):entity} to {newName}");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Returns the full name of the shuttle component in the form of [prefix] [name] [suffix].
    /// </summary>
    public static string GetFullName(ShuttleDeedComponent comp)
    {
        string?[] parts = { comp.ShuttleName, comp.ShuttleNameSuffix };
        return string.Join(' ', parts.Where(it => it != null));
    }

     private void OnPurchaseMessage(EntityUid uid, ShipyardConsoleComponent component, ShipyardConsolePurchaseMessage args)
    {
        if (args.Actor is not { Valid : true } player)
            return;

        if (!_crescent.GetPlayerIdEntity(args.Actor, out var idCardUid) ||
            !TryComp<IdCardComponent>(idCardUid, out var idCardComponent))

        {
            ConsolePopup(args.Actor, Loc.GetString("shipyard-console-no-idcard"));
            PlayDenySound(uid, component);
            return;
        }


        if (TryComp<AccessReaderComponent>(uid, out var accessReaderComponent) && !_access.IsAllowed(player, uid, accessReaderComponent))
        {
            ConsolePopup(args.Actor, Loc.GetString("comms-console-permission-denied"));
            PlayDenySound(uid, component);
            return;
        }

        if (!_prototypeManager.TryIndex<VesselPrototype>(args.Vessel, out var vessel))
        {
            ConsolePopup(args.Actor, Loc.GetString("shipyard-console-invalid-vessel", ("vessel", args.Vessel)));
            PlayDenySound(uid, component);
            return;
        }

        if (!GetAvailableShuttles(uid).Contains(vessel.ID))
        {
            PlayDenySound(uid, component);
            _adminLogger.Add(LogType.Action, LogImpact.Medium, $"{ToPrettyString(player):player} tried to purchase a vessel that was never available.");
            return;
        }

        var name = vessel.Name;
        if (vessel.Price <= 0)
            return;

        if (_station.GetOwningStation(uid) is not { Valid: true } station)
        {
            ConsolePopup(args.Actor, Loc.GetString("shipyard-console-invalid-station"));
            PlayDenySound(uid, component);
            return;
        }

        if (!TryComp<BankAccountComponent>(player, out var bank))
        {
            ConsolePopup(args.Actor, Loc.GetString("shipyard-console-no-bank"));
            PlayDenySound(uid, component);
            return;
        }

        if (bank.Balance <= vessel.Price)
        {
            ConsolePopup(args.Actor, Loc.GetString("cargo-console-insufficient-funds", ("cost", vessel.Price)));
            PlayDenySound(uid, component);
            return;
        }

        if (!_bank.TryBankWithdraw(player, vessel.Price))
        {
            ConsolePopup(args.Actor, Loc.GetString("cargo-console-insufficient-funds", ("cost", vessel.Price)));
            PlayDenySound(uid, component);
            return;
        }

        if (!TryPurchaseShuttle((EntityUid) station, vessel.Path.ToString(), out var shuttle, out var config))
        {
            PlayDenySound(uid, component);
            return;
        }
        EntityUid? shuttleStation = null;
        // setting up any stations if we have a matching game map prototype to allow late joins directly onto the vessel
        if (_prototypeManager.TryIndex<GameMapPrototype>(vessel.ID, out var stationProto))
        {
            List<EntityUid> gridUids = new()
            {
                shuttle.Owner
            };
            shuttleStation = _station.InitializeNewStation(stationProto.Stations[vessel.ID], gridUids);
            var metaData = MetaData((EntityUid) shuttleStation);
            name = metaData.EntityName;
            _shuttle.SetIFFColor(shuttle.Owner, new Color
            {
                R = 10,
                G = 50,
                B = 100,
                A = 100
            });
            _shuttle.AddIFFFlag(shuttle.Owner, IFFFlags.IsPlayerShuttle);

            // match our IFF faction with our spawner's
            if (TryComp<IFFComponent>(Transform(uid).GridUid, out var stationIFF))
            {
                _shuttle.SetIFFFaction(shuttle.Owner, stationIFF.Faction);
            }

        }
        // dynamic grid acces initializing automatically if none is mapped in
        if (!HasComp<DynamicCodeHolderComponent>(shuttle.Owner))
        {
            EnsureComp<DynamicAccesGridInitializerComponent>(shuttle.Owner);
        }


        EntityUid product = EntityManager.SpawnAtPosition("ShuttleOwnershipChip", new EntityCoordinates(uid, 0, 0));
        var deedID = EnsureComp<ShuttleDeedComponent>(product);
        AssignShuttleDeedProperties(deedID, shuttle.Owner, name, player);
        _metadata.SetEntityName(product, $"{MetaData(product).EntityName} - {deedID.ShuttleName} {deedID.ShuttleNameSuffix}");
        _metadata.SetEntityDescription(product,
            $"{MetaData(product).EntityDescription} It is owned by {idCardComponent.FullName}.");
        var deedShuttle = EnsureComp<ShuttleDeedComponent>(shuttle.Owner);
        AssignShuttleDeedProperties(deedShuttle, shuttle.Owner, name, player);

        var channel = component.ShipyardChannel;
        _handsSystem.PickupOrDrop(args.Actor, product);




        int sellValue = 0;
        if (TryComp<ShuttleDeedComponent>(product, out var deed))
            sellValue = (int) _pricing.AppraiseGrid((EntityUid) (deed?.ShuttleUid!));

        EnsureComp<ShipSpeedByMassAdjusterComponent>(shuttle.Owner);
        if (TryComp<DynamicCodeHolderComponent>(shuttle.Owner, out var shuttleCodes))
        {
            var idCodeHolder = EnsureComp<DynamicCodeHolderComponent>(idCardUid.Value);
            _codes.AddKeyToComponent(idCodeHolder, shuttleCodes.codes, null);
            Dirty(idCardUid.Value, idCodeHolder);
        }
        SendPurchaseMessage(uid, player, name, channel, false);

        ChatPurchaseLocation(uid, station, config);

        PlayConfirmSound(uid, component);
        _adminLogger.Add(LogType.ShipYardUsage, LogImpact.Low, $"{ToPrettyString(player):actor} purchased shuttle {ToPrettyString(shuttle.Owner)} for {vessel.Price} credits via {ToPrettyString(component.Owner)}");
        RefreshState(uid, bank.Balance, true, name, sellValue, true, (ShipyardConsoleUiKey) args.UiKey);

    }

    private void TryParseShuttleName(ShuttleDeedComponent deed, string name)
    {
        // The logic behind this is: if a name part fits the requirements, it is the required part. Otherwise it's the name.
        // This may cause problems but ONLY when renaming a ship. It will still display properly regardless of this.
        var nameParts = name.Split(' ');

        var hasSuffix = nameParts.Length > 1 && nameParts.Last().Length < 6 && nameParts.Last().Contains('-');
        deed.ShuttleNameSuffix = hasSuffix ? nameParts.Last() : null;
        deed.ShuttleName = System.String.Join(" ", nameParts.SkipLast(hasSuffix ? 1 : 0));
    }

    public void OnSellMessage(EntityUid uid, ShipyardConsoleComponent component, ShipyardConsoleSellMessage args)
    {

        if (args.Actor is not { Valid: true } player)
            return;

        if (component.TargetIdSlot.ContainerSlot?.ContainedEntity is not { Valid: true } targetId)
        {
            ConsolePopup(args.Actor, Loc.GetString("shipyard-console-no-idcard"));
            PlayDenySound(uid, component);
            return;
        }

        if (!TryComp<ShuttleDeedComponent>(targetId, out var deed) || deed.ShuttleUid is not { Valid : true } shuttleUid)
        {
            ConsolePopup(args.Actor, Loc.GetString("shipyard-console-no-deed"));
            PlayDenySound(uid, component);
            return;
        }

        if (!TryComp<BankAccountComponent>(player, out var bank))
        {
            ConsolePopup(args.Actor, Loc.GetString("shipyard-console-no-bank"));
            PlayDenySound(uid, component);
            return;
        }

        if (_station.GetOwningStation(uid) is not { Valid : true } stationUid)
        {
            ConsolePopup(args.Actor, Loc.GetString("shipyard-console-invalid-station"));
            PlayDenySound(uid, component);
            return;
        }


        var shuttleName = ToPrettyString(shuttleUid); // Grab the name before it gets 1984'd

        var channel = component.ShipyardChannel;

        if (!TrySellShuttle(stationUid, shuttleUid, out var bill))
        {
            ConsolePopup(args.Actor, Loc.GetString("shipyard-console-sale-reqs"));
            PlayDenySound(uid, component);
            return;
        }

        RemComp<ShuttleDeedComponent>(targetId);
        Del(targetId);


        _bank.TryBankDeposit(player, bill);
        PlayConfirmSound(uid, component);

        SendSellMessage(uid, deed.ShuttleOwner!, GetFullName(deed), channel, player, false);

        _adminLogger.Add(LogType.ShipYardUsage, LogImpact.Low, $"{ToPrettyString(player):actor} sold {shuttleName} for {bill} credits via {ToPrettyString(component.Owner)}");
        RefreshState(uid, bank.Balance, true, null, 0, true, (ShipyardConsoleUiKey) args.UiKey);
    }

    private void OnConsoleUIOpened(EntityUid uid, ShipyardConsoleComponent component, BoundUIOpenedEvent args)
    {
        // kind of cursed. We need to update the UI when an Id is entered, but the UI needs to know the player characters bank account.
        if (!TryComp<ActivatableUIComponent>(uid, out var uiComp) || uiComp.Key == null)
            return;

        if (args.Actor is not { Valid: true } player)
            return;

        //      mayhaps re-enable this later for HoS/SA
        //        var station = _station.GetOwningStation(uid);

        if (!TryComp<BankAccountComponent>(player, out var bank))
            return;

        var targetId = component.TargetIdSlot.ContainerSlot?.ContainedEntity;

        if (TryComp<ShuttleDeedComponent>(targetId, out var deed))
        {
            if (Deleted(deed.ShuttleUid))
            {
                RemComp<ShuttleDeedComponent>(targetId.Value);
                return;
            }
        }

        int sellValue = 0;
        if (deed?.ShuttleUid != null)
            sellValue = (int) _pricing.AppraiseGrid((EntityUid) (deed?.ShuttleUid!));

        if (ShipyardConsoleUiKey.BlackMarket == (ShipyardConsoleUiKey) args.UiKey || ShipyardConsoleUiKey.Syndicate == (ShipyardConsoleUiKey) args.UiKey) // Unhardcode this please
        {
            var tax = (int) (sellValue * 0.30f);
            sellValue -= tax;
        }

        var fullName = deed != null ? GetFullName(deed) : null;
        RefreshState(uid, bank.Balance, true, fullName, sellValue, targetId.HasValue, (ShipyardConsoleUiKey) args.UiKey);
    }

    private void ConsolePopup(EntityUid uid, string text)
    {
            _popup.PopupEntity(text, uid);
    }

    private void SendPurchaseMessage(EntityUid uid, EntityUid player, string name, string shipyardChannel, bool secret)
    {
        var channel = _prototypeManager.Index<RadioChannelPrototype>(shipyardChannel);

        if (secret)
        {
            _radio.SendRadioMessage(uid, Loc.GetString("shipyard-console-docking-secret"), channel, uid);
            _chat.TrySendInGameICMessage(uid, Loc.GetString("shipyard-console-docking-secret"), InGameICChatType.Speak, true);
        }
        else
        {
            _radio.SendRadioMessage(uid, Loc.GetString("shipyard-console-docking", ("owner", player), ("vessel", name)), channel, uid);
            _chat.TrySendInGameICMessage(uid, Loc.GetString("shipyard-console-docking", ("owner", player!), ("vessel", name)), InGameICChatType.Speak, true);
        }
    }

    private void ChatPurchaseLocation(EntityUid chatter, EntityUid station, DockingConfig? config)
    {
        // Null config means we didn't dock and had to park nearby.
        if (config == null)
        {
            _chat.TrySendInGameICMessage(chatter, "Your ship has been towed in local station space. Fly to it using a jetpack and a mass scanner!", InGameICChatType.Speak, false);
            return;
        }

        var grid = config.TargetGrid;
        var dock = config.Docks[0].DockBUid;

        // The dock needs to be parented to the target grid, and the target grid should actually exist.
        if (Transform(dock).GridUid != grid || !TryComp<MapGridComponent>(grid, out var mapGrid))
        {
            _sawmill.Error("Cannot get docking location for " + EntityManager.ToPrettyString(chatter));
            return;
        }

        //Now we figure out where in the map grid the dock is.
        var pos = Transform(dock).LocalPosition;
        var center = mapGrid.LocalAABB.Center;

        var dir = pos - center;

        var angle = dir.ToAngle().Degrees + 180;

        string direction = angle switch
        {
            <= 15f => "9",
            <= 45f => "8",
            <= 75f => "7",
            <= 105f => "6",
            <= 135f => "5",
            <= 165f => "4",
            <= 195f => "3",
            <= 225f => "2",
            <= 255f => "1",
            <= 285f => "12",
            <= 315f => "11",
            <= 345f => "10",
            _ => "9",
        };

        _chat.TrySendInGameICMessage(chatter, Loc.GetString("shipyard-console-direction", ("direction", direction.ToLower()), ("station", station)), InGameICChatType.Speak, false);
    }

    private void SendSellMessage(EntityUid uid, EntityUid? player, string name, string shipyardChannel, EntityUid seller, bool secret)
    {
        var channel = _prototypeManager.Index<RadioChannelPrototype>(shipyardChannel);

        if (secret)
        {
            _radio.SendRadioMessage(uid, Loc.GetString("shipyard-console-leaving-secret"), channel, uid);
            _chat.TrySendInGameICMessage(uid, Loc.GetString("shipyard-console-leaving-secret"), InGameICChatType.Speak, true);
        }
        else
        {
            _radio.SendRadioMessage(uid, Loc.GetString("shipyard-console-leaving", ("owner", player!), ("vessel", name!), ("player", seller)), channel, uid);
            _chat.TrySendInGameICMessage(uid, Loc.GetString("shipyard-console-leaving", ("owner", player!), ("vessel", name!), ("player", seller)), InGameICChatType.Speak, true);
        }
    }

    private void PlayDenySound(EntityUid uid, ShipyardConsoleComponent component)
    {
        _audio.PlayPvs(_audio.GetSound(component.DenySound), uid, AudioParams.Default.WithMaxDistance(0.01f));
    }

    private void PlayConfirmSound(EntityUid uid, ShipyardConsoleComponent component)
    {
        _audio.PlayPvs(_audio.GetSound(component.ConfirmSound), uid, AudioParams.Default.WithMaxDistance(0.01f));
    }

    private void OnItemSlotChanged(EntityUid uid, ShipyardConsoleComponent component, ContainerModifiedMessage args)
    {
        if (!TryComp<ActivatableUIComponent>(uid, out var uiComp) || uiComp.Key == null)
            return;

        var uiUsers = _ui.GetActors(uid, uiComp.Key);

        foreach (var user in uiUsers)
        {
            if (user is not { Valid: true } player)
                continue;

            if (!TryComp<BankAccountComponent>(player, out var bank))
                continue;

            var targetId = component.TargetIdSlot.ContainerSlot?.ContainedEntity;
            ShuttleDeedComponent? deed = null;

            if (targetId.HasValue && TryComp(targetId.Value, out deed))
            {
                if (Deleted(deed.ShuttleUid))
                {
                    RemComp<ShuttleDeedComponent>(targetId.Value);
                    continue;
                }
            }

            var sellValue = deed?.ShuttleUid != null
                ? (int)_pricing.AppraiseGrid((EntityUid)deed.ShuttleUid)
                : 0;

            if (uiComp.Key is ShipyardConsoleUiKey.BlackMarket
                or ShipyardConsoleUiKey.Syndicate)
            {
                var tax = (int)(sellValue * 0.30f);
                sellValue -= tax;
            }

            var fullName = deed != null ? GetFullName(deed) : null;
            RefreshState(uid, bank.Balance, true, fullName, sellValue, targetId.HasValue, (ShipyardConsoleUiKey)uiComp.Key);
            RefreshState(
                uid: uid,
                balance: bank.Balance,
                access: true,
                shipDeed: fullName,
                shipSellValue: sellValue,
                isTargetIdPresent: targetId.HasValue,
                (ShipyardConsoleUiKey)uiComp.Key);
        }
    }

    public bool FoundOrganics(EntityUid uid, EntityQuery<MobStateComponent> mobQuery, EntityQuery<TransformComponent> xformQuery)
    {
        var xform = xformQuery.GetComponent(uid);
        var childEnumerator = xform.ChildEnumerator;

        while (childEnumerator.MoveNext(out var child))
        {
            if (mobQuery.TryGetComponent(child, out var mobState)
                && !_mobState.IsDead(child, mobState)
                && _mind.TryGetMind(child, out var mind, out var mindComp)
                && !_mind.IsCharacterDeadIc(mindComp)
                || FoundOrganics(child, mobQuery, xformQuery))
                return true;
        }

        return false;
    }

    /// <summary>
    ///   Returns all shuttle prototype IDs the given shipyard console can offer.
    /// </summary>
    public List<string> GetAvailableShuttles(EntityUid uid, ShipyardConsoleUiKey? key = null, ShipyardListingComponent? listing = null)
    {
        var availableShuttles = new List<string>();

        if (key == null && TryComp<UserInterfaceComponent>(uid, out var ui))
        {
            // Try to find a ui key that is an instance of the shipyard console ui key
            foreach (var (k, v) in ui.Actors)
            {
                if (k is ShipyardConsoleUiKey shipyardKey)
                {
                    key = shipyardKey;
                    break;
                }
            }
        }

        // Add all prototypes matching the ui key
        if (key != null && key != ShipyardConsoleUiKey.Custom && ShipyardGroupMapping.TryGetValue(key.Value, out var group))
        {
            var protos = _prototypeManager.EnumeratePrototypes<VesselPrototype>();
            foreach (var proto in protos)
            {
                if(proto.Whitelist is not null && _whitelists.IsValid(proto.Whitelist, uid))
                    availableShuttles.Add(proto.ID);
            }
        }

        // Add all prototypes specified in ShipyardListing
        if (listing != null || TryComp(uid, out listing))
        {
            foreach (var shuttle in listing.Shuttles)
            {
                availableShuttles.Add(shuttle);
            }
        }

        return availableShuttles;
    }

    private void RefreshState(EntityUid uid, long balance, bool access, string? shipDeed, int shipSellValue, bool isTargetIdPresent, ShipyardConsoleUiKey uiKey)
    {
        var listing = TryComp<ShipyardListingComponent>(uid, out var comp) ? comp : null;

        var newState = new ShipyardConsoleInterfaceState(
            balance,
            access,
            shipDeed,
            shipSellValue,
            isTargetIdPresent,
            ((byte)uiKey),
            GetAvailableShuttles(uid, uiKey, listing),
            uiKey.ToString());

        _ui.SetUiState(uid, uiKey, newState);
    }

    void AssignShuttleDeedProperties(ShuttleDeedComponent deed, EntityUid? shuttleUid, string? shuttleName, EntityUid? shuttleOwner)
    {
        deed.ShuttleUid = shuttleUid;
        TryParseShuttleName(deed, shuttleName!);
        deed.ShuttleOwner = shuttleOwner;
        if(shuttleUid is not null)
            EntityManager.Dirty(shuttleUid.Value, deed);
    }

    private void OnInitDeedSpawner(EntityUid uid, StationDeedSpawnerComponent component, MapInitEvent args)
    {
        if (!HasComp<IdCardComponent>(uid)) // Test if the deed on an ID
            return;

        var xform = Transform(uid); // Get the grid the card is on
        if (xform.GridUid == null)
            return;

        if (!TryComp<ShuttleDeedComponent>(xform.GridUid.Value, out var shuttleDeed) || !TryComp<ShuttleComponent>(xform.GridUid.Value, out var shuttle) || !HasComp<TransformComponent>(xform.GridUid.Value) || shuttle == null  || ShipyardMap == null)
            return;

        var shuttleOwner = ToPrettyString(shuttleDeed.ShuttleOwner); // Grab owner name
        var output = Regex.Replace($"{shuttleOwner}", @"\s*\([^()]*\)", ""); // Removes content inside parentheses along with parentheses and a preceding space
        _idSystem.TryChangeFullName(uid, output); // Update the card with owner name

        var deedID = EnsureComp<ShuttleDeedComponent>(uid);
        AssignShuttleDeedProperties(deedID, shuttleDeed.ShuttleUid, shuttleDeed.ShuttleName, shuttleDeed.ShuttleOwner);
    }

    private void OnInteractUsing(EntityUid uid, ShipyardConsoleComponent component, InteractUsingEvent args)
    {
        if (args.Handled)
        {
            return;
        }

        if (TryComp<ShipVoucherComponent>(args.Used, out var voucher) && !string.IsNullOrEmpty(voucher.Ship))
        {
            args.Handled = true;
            if (TryRedeemShip(uid, component, args.User, voucher))
            {
                QueueDel(args.Used);
            }
        }
    }

    private bool TryRedeemShip(EntityUid uid, ShipyardConsoleComponent component, EntityUid user, ShipVoucherComponent voucher)
    {
        if (!TryComp<ActivatableUIComponent>(uid, out var ui) || ui.Key == null)
        {
            return false;
        }

        if (!_crescent.GetPlayerIdEntity(user, out var idCardUid) ||
            !TryComp<AccessComponent>(idCardUid, out var accesComp) ||
            !TryComp<IdCardComponent>(idCardUid, out var idCardComponent))

        {
            ConsolePopup(user, Loc.GetString("shipyard-console-no-idcard"));
            PlayDenySound(uid, component);
            return false;
        }


        if (TryComp<AccessReaderComponent>(uid, out var accessReaderComponent) && !_access.IsAllowed(user, uid, accessReaderComponent))
        {
            ConsolePopup(user, Loc.GetString("comms-console-permission-denied"));
            PlayDenySound(uid, component);
            return false;
        }

        if (!_prototypeManager.TryIndex<VesselPrototype>(voucher.Ship, out var vessel))
        {
            ConsolePopup(user, Loc.GetString("shipyard-console-invalid-vessel", ("vessel", voucher.Ship)));
            PlayDenySound(uid, component);
            return false;
        }

        if (voucher.RequiresShipInConsole)
        {
            if (!GetAvailableShuttles(uid).Contains(vessel.ID))
            {
                PlayDenySound(uid, component);
                _adminLogger.Add(LogType.Action, LogImpact.Medium, $"{ToPrettyString(user):player} tried to redeem a vessel that was never available.");
                return false;
            }
        }

        var name = vessel.Name;

        if (_station.GetOwningStation(uid) is not { Valid: true } station)
        {
            ConsolePopup(user, Loc.GetString("shipyard-console-invalid-station"));
            PlayDenySound(uid, component);
            return false;
        }

        if (!TryPurchaseShuttle((EntityUid) station, vessel.Path.ToString(), out var shuttle, out var config))
        {
            PlayDenySound(uid, component);
            return false;
        }

        EntityUid? shuttleStation = null;
        // setting up any stations if we have a matching game map prototype to allow late joins directly onto the vessel
        if (_prototypeManager.TryIndex<GameMapPrototype>(vessel.ID, out var stationProto))
        {
            List<EntityUid> gridUids = new()
            {
                shuttle.Owner
            };
            shuttleStation = _station.InitializeNewStation(stationProto.Stations[vessel.ID], gridUids);
            var metaData = MetaData((EntityUid) shuttleStation);
            name = metaData.EntityName;
            _shuttle.SetIFFColor(shuttle.Owner, new Color
            {
                R = 10,
                G = 50,
                B = 100,
                A = 100
            });
            _shuttle.AddIFFFlag(shuttle.Owner, IFFFlags.IsPlayerShuttle);
            var comp = EnsureComp<ShipPriceMultiplierComponent>(shuttle.Owner);
            comp.priceMultiplier = 0.25f;

            // match our IFF faction with our spawner's
            if (TryComp<IFFComponent>(Transform(uid).GridUid, out var stationIFF))
            {
                _shuttle.SetIFFFaction(shuttle.Owner, stationIFF.Faction);
            }
        }
        // dynamic grid acces initializing automatically if none is mapped in
        if (!HasComp<DynamicCodeHolderComponent>(shuttle.Owner))
        {
            EnsureComp<DynamicAccesGridInitializerComponent>(shuttle.Owner);
        }

        EntityUid product = EntityManager.SpawnAtPosition("ShuttleOwnershipChip", new EntityCoordinates(uid, 0, 0));
        var deedID = EnsureComp<ShuttleDeedComponent>(product);
        AssignShuttleDeedProperties(deedID, shuttle.Owner, name,  user);
        _metadata.SetEntityName(product, $"{MetaData(product).EntityName} - {deedID.ShuttleName} {deedID.ShuttleNameSuffix}");
        _metadata.SetEntityDescription(product, $"{MetaData(product).EntityDescription} It is owned by {idCardComponent.FullName}.");

        var deedShuttle = EnsureComp<ShuttleDeedComponent>(shuttle.Owner);
        AssignShuttleDeedProperties(deedShuttle, shuttle.Owner, name, user);

        var channel = component.ShipyardChannel;


        int sellValue = 0;
        if (TryComp<ShuttleDeedComponent>(product, out var deed))
            sellValue = (int) _pricing.AppraiseGrid((EntityUid) (deed?.ShuttleUid!));

        EnsureComp<ShipSpeedByMassAdjusterComponent>(shuttle.Owner);
        if (TryComp<DynamicCodeHolderComponent>(shuttle.Owner, out var shuttleCodes))
        {
            var idCodeHolder = EnsureComp<DynamicCodeHolderComponent>(idCardUid.Value);
            _codes.AddKeyToComponent(idCodeHolder, shuttleCodes.codes, null);
            Dirty(idCardUid.Value, idCodeHolder);
        }


        SendPurchaseMessage(uid, user, name, channel, false);

        ChatPurchaseLocation(uid, station, config);

        PlayConfirmSound(uid, component);
        _adminLogger.Add(LogType.ShipYardUsage, LogImpact.Low, $"{ToPrettyString(user):actor} redeemed shuttle {ToPrettyString(shuttle.Owner)} with voucher via {ToPrettyString(component.Owner)}");

        return true;
    }
}

