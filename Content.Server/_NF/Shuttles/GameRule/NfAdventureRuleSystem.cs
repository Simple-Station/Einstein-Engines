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
using Robust.Server.Maps;
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

    [ViewVariables]
    // this is used for money but its very poorly named - SPCR 2025
    private List<(EntityUid, long)> _players = new();

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

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

            var profit = (long)bank.Balance - player.Item2;
            ev.AddLine($"- {meta.EntityName} {profitText} {profit} Credits");
            allScore.Add(new Tuple<string, int>(meta.EntityName, (int)profit));
        }

        if (!(allScore.Count >= 1))
            return;

        var relayText = Loc.GetString("adventure-list-high");
        relayText += '\n';
        var highScore = allScore.OrderByDescending(h => h.Item2).ToList();

        for (var i = 0; i < 10 && i < highScore.Count; i++)
        {
            relayText += $"{highScore.First().Item1} {profitText} {highScore.First().Item2.ToString()} Credits";
            relayText += '\n';
            highScore.Remove(highScore.First());
        }
        relayText += Loc.GetString("adventure-list-low");
        relayText += '\n';
        highScore.Reverse();
        for (var i = 0; i < 10 && i < highScore.Count; i++)
        {
            relayText += $"{highScore.First().Item1} {lossText} {highScore.First().Item2.ToString()} Credits";
            relayText += '\n';
            highScore.Remove(highScore.First());
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

    protected override void Started(EntityUid uid, AdventureRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        var unionfall_clementine = "/Maps/_Crescent/Unionfall/unionfall_clementine.yml";
        var unionfall_nemesis = "/Maps/_Crescent/Unionfall/unionfall_nemesis.yml";
        var unionfall_vladzena = "/Maps/_Crescent/Unionfall/unionfall_vladzena.yml";
        var unionfall_grace_barrier_ncwl = "/Maps/_Crescent/Unionfall/grace_barrier_NCWL.yml";
        var unionfall_grace_barrier_dsm = "/Maps/_Crescent/Unionfall/grace_barrier_DSM.yml";
        var unionfall_biome_emitter_ncwl = "/Maps/_Crescent/Unionfall/unionfall_biome_emitter_NCWL.yml";
        var unionfall_biome_emitter_dsm = "/Maps/_Crescent/Unionfall/unionfall_biome_emitter_DSM.yml";

        var depotColor = new Color(55, 200, 55);
        var civilianColor = new Color(55, 55, 200);
        var lpbravoColor = new Color(200, 55, 55);
        var coveColor = new Color(203, 195, 227);
        var tatsumotoColor = new Color(128, 128, 128);
        var factionColor = new Color(255, 165, 0);
        var refugeColor = new Color(34, 139, 34);
        var mapId = GameTicker.DefaultMap;
        var depotOffset = _random.NextVector2(4500f, 6000f);
        var tinniaOffset = _random.NextVector2(12100f, 5800f);
        var caseysOffset = _random.NextVector2(2250f, 4600f);
        var tradeOffset = _random.NextVector2(1500f, 2500f);

        // CONSCRIPT - UNIONFALL
        // THIS SHOULD BE REPLACED ONCE WE HAVE THE NEW GAMEMODE REFACTOR!
        // CONSCRIPT - UNIONFALL


        //UNIONFALL_NEMESIS
        if (_map.TryLoad(mapId, unionfall_nemesis, out var nemesisUid, new MapLoadOptions
        {
            Offset = new Vector2(2000f, 4500f)
        }))
        {
            if (_prototypeManager.TryIndex<GameMapPrototype>("unionfall-Nemesis", out var stationProto))
            {
                _station.InitializeNewStation(stationProto.Stations["Unionfall-Nemesis"], nemesisUid);
            }

            var meta = EnsureComp<MetaDataComponent>(nemesisUid[0]);
            _meta.SetEntityName(nemesisUid[0], "DSM Nemesis-P", meta);
            _shuttle.SetIFFColor(nemesisUid[0], lpbravoColor);
        }

        //UNIONFALL_CLEMENTINE
        if (_map.TryLoad(mapId, unionfall_clementine, out var clementineUid, new MapLoadOptions
        {
            Offset = new Vector2(-2000f, 4500f)
        }))
        {
            if (_prototypeManager.TryIndex<GameMapPrototype>("unionfall-Clementine", out var stationProto))
            {
                _station.InitializeNewStation(stationProto.Stations["Unionfall-Clementine"], clementineUid);
            }

            var meta = EnsureComp<MetaDataComponent>(clementineUid[0]);
            _meta.SetEntityName(clementineUid[0], "NCWL Dear Clementine", meta);
            _shuttle.SetIFFColor(clementineUid[0], factionColor);
        }

        //UNIONFALL_VLADZENA
        if (_map.TryLoad(mapId, unionfall_vladzena, out var vladzenaUid, new MapLoadOptions
        {
            Offset = new Vector2(0f, 9000f)
        }))
        {
            if (_prototypeManager.TryIndex<GameMapPrototype>("unionfall-Vladzena", out var stationProto))
            {
                _station.InitializeNewStation(stationProto.Stations["Unionfall-Vladzena"], vladzenaUid);
            }

            var meta = EnsureComp<MetaDataComponent>(vladzenaUid[0]);
            _meta.SetEntityName(vladzenaUid[0], "NT Outpost Vladzena", meta);
            _shuttle.SetIFFColor(vladzenaUid[0], depotColor);
        }

        //BARRIER FOR CLEMENTINE - SHOULD MATCH CLEMENTINE COORDINATES BECAUSE ITS A DONUT
        if (_map.TryLoad(mapId, unionfall_grace_barrier_ncwl, out var barrierUidA, new MapLoadOptions
        {
            Offset = new Vector2(-2000f, 4500f)
        }))
        {
            if (_prototypeManager.TryIndex<GameMapPrototype>("unionfall-Grace-Barrier-NCWL", out var stationProto))
            {
                _station.InitializeNewStation(stationProto.Stations["Unionfall-Grace-Barrier-NCWL"], barrierUidA);
            }

            var meta = EnsureComp<MetaDataComponent>(barrierUidA[0]);
            _meta.SetEntityName(barrierUidA[0], "Hadal Protection Bubble (NCWL)", meta);
            _shuttle.SetIFFColor(barrierUidA[0], depotColor);
        }

        //BARRIER FOR NEMESIS - SHOULD MATCH NEMESIS COORDINATES BECAUSE ITS A DONUT
        if (_map.TryLoad(mapId, unionfall_grace_barrier_dsm, out var barrierUidB, new MapLoadOptions
        {
            Offset = new Vector2(2000f, 4500f)
        }))
        {
            if (_prototypeManager.TryIndex<GameMapPrototype>("unionfall-Grace-Barrier-DSM", out var stationProto))
            {
                _station.InitializeNewStation(stationProto.Stations["Unionfall-Grace-Barrier-DSM"], barrierUidB);
            }

            var meta = EnsureComp<MetaDataComponent>(barrierUidB[0]);
            _meta.SetEntityName(barrierUidB[0], "Hadal Protection Bubble (DSM)", meta);
            _shuttle.SetIFFColor(barrierUidB[0], depotColor);
        }

        //FIGHT BIOME EMITTER FOR DSM - COORDINATES HALFWAY BETWEEN VLAD AND NEM
        if (_map.TryLoad(mapId, unionfall_biome_emitter_dsm, out var biomeEmitterUidA, new MapLoadOptions
        {
            Offset = new Vector2(1000f, 6750f)
        }))
        {
            if (_prototypeManager.TryIndex<GameMapPrototype>("unionfall-Biome-Emitter-DSM", out var stationProto))
            {
                _station.InitializeNewStation(stationProto.Stations["Unionfall-Biome-Emitter-DSM"], biomeEmitterUidA);
            }
            var meta = EnsureComp<MetaDataComponent>(biomeEmitterUidA[0]);
            _meta.SetEntityName(biomeEmitterUidA[0], "Biome Emitter DSM", meta);
            _shuttle.SetIFFColor(biomeEmitterUidA[0], depotColor);
            _shuttle.AddIFFFlag(biomeEmitterUidA[0], IFFFlags.HideLabel);
        }

        //FIGHT BIOME EMITTER FOR NCWL - COORDINATES HALFWAY BETWEEN VLAD AND CLEM
        if (_map.TryLoad(mapId, unionfall_biome_emitter_ncwl, out var biomeEmitterUidB, new MapLoadOptions
        {
            Offset = new Vector2(-1000f, 6750f)
        }))
        {
            if (_prototypeManager.TryIndex<GameMapPrototype>("unionfall-Biome-Emitter-NCWL", out var stationProto))
            {
                _station.InitializeNewStation(stationProto.Stations["Unionfall-Biome-Emitter-NCWL"], biomeEmitterUidB);
            }
            var meta = EnsureComp<MetaDataComponent>(biomeEmitterUidB[0]);
            _meta.SetEntityName(biomeEmitterUidB[0], "Biome Emitter NCWL", meta);
            _shuttle.SetIFFColor(biomeEmitterUidB[0], depotColor);
            _shuttle.AddIFFFlag(biomeEmitterUidB[0], IFFFlags.HideLabel);
        }
    }
}
