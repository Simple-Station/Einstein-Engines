using System.Linq;
using System.Net.Http;
using System.Text;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Content.Server.Procedural;
using Content.Shared.Bank.Components;
using Content.Server.GameTicking.Events;
using Content.Server.GameTicking.Rules.Components;
using Content.Shared.Procedural;
using Robust.Server.GameObjects;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.EntitySerialization;
using Robust.Shared.Console;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Map.Components;
using Content.Shared.Shuttles.Components;
using Content.Server.Shuttles.Systems;
using Content.Server.Cargo.Components;
using Content.Server.GameTicking;
using Content.Server.Maps;
using Content.Server.Station.Systems;
using Content.Shared.CCVar;
using Content.Shared.GameTicking;
using Content.Shared.GameTicking.Components;
using Robust.Shared.Configuration;
using Content.Shared.Telescope;
using Robust.Shared.Utility;

namespace Content.Server.GameTicking.Rules;

/// <summary>
/// This handles the dungeon and trading post spawning, as well as round end capitalism summary
/// </summary>
public sealed class NfAdventureRuleSystem : GameRuleSystem<AdventureRuleComponent>
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;
    [Dependency] private readonly MapLoaderSystem _map = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly DungeonSystem _dunGen = default!;
    [Dependency] private readonly IConsoleHost _console = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly ShuttleSystem _shuttle = default!;

    private readonly HttpClient _httpClient = new();
    private ISawmill _sawmill = default!;

    [ViewVariables]
    // this is used for money but its very poorly named - SPCR 2025
    private List<(EntityUid, long)> _players = new();

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        _sawmill = IoCManager.Resolve<ILogManager>().GetSawmill("nfadventurerulesystem");

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawningEvent);
    }

    protected override void AppendRoundEndText(EntityUid uid, AdventureRuleComponent component, GameRuleComponent gameRule, ref RoundEndTextAppendEvent ev)
    {
        var profitText = Loc.GetString($"adventure-mode-profit-text");
        var lossText = Loc.GetString($"adventure-mode-loss-text");
        ev.AddLine(Loc.GetString("adventure-list-start"));
        var allScore = new List<Tuple<string, int>>();

        foreach (var player in _players)
        {
            if (!TryComp<BankAccountComponent>(player.Item1, out var bank) || !TryComp<MetaDataComponent>(player.Item1, out var meta))
                continue;

            var profit = (long) bank.Balance - player.Item2;
            allScore.Add(new Tuple<string, int>(meta.EntityName, (int) profit));
        }

        if (!(allScore.Count >= 1))
            return;

        // Sort by profit (highest first) and display all players
        var sortedScores = allScore.OrderByDescending(h => h.Item2).ToList();

        foreach (var score in sortedScores)
        {
            var displayText = score.Item2 < 0 ? lossText : profitText;
            ev.AddLine($"- {score.Item1} {displayText} {score.Item2} Credits");
        }
    }

    private void OnPlayerSpawningEvent(PlayerSpawnCompleteEvent ev)
    {
        if (ev.Player.AttachedEntity is { Valid: true } mobUid)
        {
            _players.Add((mobUid, ev.Profile.BankBalance));
            EnsureComp<CargoSellBlacklistComponent>(mobUid);

        }
    }

    /// <summary>
    /// This is a helper function that spawns in stuff by their gameMap .yml's ID field. The map's path is fetched from the gameMap .yml
    /// </summary>
    /// <param name="mapid"> the ID of the map. this is always GameTicker.DefaultMap; for hullrot </param>
    /// <param name="gameMapID">the ID of the gameMap prototype to spawn</param>
    /// <param name="posX">the X coordinate to spawn it at</param>
    /// <param name="posY">the Y coordinate to spawn it at</param>
    /// <param name="randomOffsetX">the maximum POSITIVE value the X coordinate can be offset by randomly. 0 works</param>
    /// <param name="randomOffsetY">the maximum POSITIVE value the Y coordinate can be offset by randomly. 0 works</param>
    /// <param name="color">the IFF color to set this object to</param>
    /// <param name="IFFFaction">the IFF faction to set this to. i don't think this does anything</param>
    /// <param name="hideIFF">a boolean to set wether this is visible on the map screen or not</param>
    private void SpawnMapElementByID(MapId mapid, string gameMapID, float posX, float posY, float randomOffsetX, float randomOffsetY, Color color, string? iffFaction, bool hideIFF)
    {
        if (_prototypeManager.TryIndex<GameMapPrototype>(gameMapID, out var stationProto))
        {
            if (_map.TryLoadGrid(mapid, new ResPath(stationProto.MapPath.ToString()), out var stationGridUid, null, new Vector2(posX, posY) + _random.NextVector2(randomOffsetX, randomOffsetY)))
            {
                _station.InitializeNewStation(stationProto.Stations[gameMapID], [stationGridUid.Value.Owner]);

                // setting color if applicable. if not, White is default
                _shuttle.SetIFFColor(stationGridUid.Value.Owner, color);

                // set IFFFaction if applicable. dont know if this does anything
                if (iffFaction != null)
                    _shuttle.SetIFFFaction(stationGridUid.Value.Owner, iffFaction);

                // hide IFF if needed, like for derelicts or secrets
                if (hideIFF)
                    _shuttle.AddIFFFlag(stationGridUid.Value.Owner, IFFFlags.HideLabel);
            }
            else
            {
                _sawmill.Error($"Failed to load {gameMapID} in map {mapid}");
            }
        }
    }

    protected override void Started(EntityUid uid, AdventureRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        var mapId = GameTicker.DefaultMap;
        base.Started(uid, component, gameRule, args);

        foreach (var gamemap in component.GameMapsID)
        {
            SpawnMapElementByID(mapId,
                                gamemap.Value.GameMapID,
                                gamemap.Value.PositionX,
                                gamemap.Value.PositionY,
                                gamemap.Value.RandomOffsetX,
                                gamemap.Value.RandomOffsetY,
                                gamemap.Value.IFFColor,
                                gamemap.Value.IFFFaction,
                                gamemap.Value.HideIFF);


            // _sawmill.Debug("------------");
            // _sawmill.Debug("GAMEMAPID: " + gamemap.Value.GameMapID);
            // _sawmill.Debug("posX: " + gamemap.Value.PositionX);
            // _sawmill.Debug("posY: " + gamemap.Value.PositionY);
            // _sawmill.Debug("------------");
        }
    }
}
