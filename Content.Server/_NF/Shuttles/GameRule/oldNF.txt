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

        var depotMap = "/Maps/_NF/POI/cargodepot.yml";
        var tinnia = "/Maps/_NF/POI/tinnia.yml";
        //var caseys = "/Maps/_NF/POI/caseyscasino.yml";
       // var lpbravo = "/Maps/_NF/POI/lpbravo.yml";
        var hayes = "/Maps/_Crescent/Explorables/hayeswreck.yml";
        //var lpramzi = "/Maps/_Crescent/Stations/lpramzi.yml";
       // var tatsumoto = "/Maps/_Crescent/Stations/tatsumoto.yml";
        var oris = "/Maps/_Crescent/Explorables/oris.yml";
        var fogwalker = "/Maps/_Crescent/Explorables/fogexplorer.yml";
        var borealis = "/Maps/_Crescent/Stations/borealis.yml";
        var cometevent = "/Maps/_Crescent/Stations/cometevent.yml";
        var taypanone = "/Maps/_Crescent/Explorables/taypanone.yml";
        var craster = "/Maps/_Crescent/Explorables/craster.yml";
        //var dochenskaya = "/Maps/_Crescent/Stations/dochenskaya.yml";
        var jackal = "/Maps/_Crescent/Stations/jackal.yml";
       // var refuge = "/Maps/_Crescent/Stations/refuge.yml";
        var vladzena = "/Maps/_Crescent/Stations/vladzena.yml";
        var defensebattery = "/Maps/_Crescent/Stations/defensebatteryimperial.yml";
        // var northpole = "/Maps/_NF/POI/northpole.yml";
        var arena = "/Maps/_Crescent/Explorables/zhipovwreck.yml";
        var aasim = "/Maps/_Crescent/Stations/aasim.yml";
        var stranded = "/Maps/_Crescent/Explorables/stranded.yml";
        var fighter1 = "/Maps/_Crescent/Explorables/ruinedfightereast.yml";
        var fighter2 = "/Maps/_Crescent/Explorables/ruinedfighterwest.yml";
        var solarruined = "/Maps/_Crescent/Explorables/ruinedsolarsailor.yml";
        var impwreck = "/Maps/_Crescent/Explorables/impwreck.yml";
        var courthouse = "/Maps/_Crescent/Stations/kalsuzerai.yml";
        var ardour = "/Maps/_Crescent/Stations/ardour.yml";
        var gliesssanto = "/Maps/_Crescent/Stations/gliess.yml";
        // var lodge = "/Maps/_NF/POI/lodge.yml";
        var lab = "/Maps/_NF/POI/anomalouslab.yml";
        // var church = "Maps/_NF/POI/beacon.yml";
        // var grifty = "Maps/_NF/POI/grifty.yml";
        var precinct9 = "/Maps/_Crescent/Stations/precinct9.yml";
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

        /*if (_map.TryLoad(mapId, depotMap, out var depotUids, new MapLoadOptions
            {
                Offset = depotOffset
            }))
        {
            var meta = EnsureComp<MetaDataComponent>(depotUids[0]);
            _meta.SetEntityName(depotUids[0], "Resupply Depot A", meta);
            _shuttle.SetIFFColor(depotUids[0], depotColor);
        }

        if (_map.TryLoad(mapId, depotMap, out var depotUid3s, new MapLoadOptions
            {
                Offset = -depotOffset
            }))
        {
            var meta = EnsureComp<MetaDataComponent>(depotUid3s[0]);
            _meta.SetEntityName(depotUid3s[0], "Resupply Depot B", meta);
            _shuttle.SetIFFColor(depotUid3s[0], depotColor);
        }*/

     //   if (_map.TryLoad(mapId, precinct9, out var nfsdUids, new MapLoadOptions
     //       {
     //           Offset = new Vector2(2500f,4500f)
     //       }))
     //   {
     //      //  We should figure out if it is possible to add this grid to the latejoin listing.
     //      //  Hey turns out we can! (This is kinda copypasted from the lodge with some values filled in.)
     //        if (_prototypeManager.TryIndex<GameMapPrototype>("Precinct9", out var stationProto))
     //       {
     //           _station.InitializeNewStation(stationProto.Stations["Precinct9"], nfsdUids);
     //       }
//
     //       var meta = EnsureComp<MetaDataComponent>(nfsdUids[0]);
     //       _meta.SetEntityName(nfsdUids[0], "TSP Proctor Annalise", meta);
     //       _shuttle.SetIFFColor(nfsdUids[0], civilianColor);
     //   }

     //   if (_map.TryLoad(mapId, borealis, out var borealisUids, new MapLoadOptions
     //   {
     //       Offset = new Vector2(100f, 400f)
    //    }))
    //    {
    //        //  We should figure out if it is possible to add this grid to the latejoin listing.
    //        //  Hey turns out we can! (This is kinda copypasted from the lodge with some values filled in.)
    //        if (_prototypeManager.TryIndex<GameMapPrototype>("Borealis", out var stationProto))
    //        {
     //           _station.InitializeNewStation(stationProto.Stations["Borealis"], borealisUids);
     //       }
//
     //       var meta = EnsureComp<MetaDataComponent>(borealisUids[0]);
     //       _meta.SetEntityName(borealisUids[0], "CMM Borealis", meta);
      //      _shuttle.SetIFFColor(borealisUids[0], civilianColor);
      //      _shuttle.SetIFFFaction(borealisUids[0], "TSP");
     //   }

    //    if (_map.TryLoad(mapId, cometevent, out var cometUids, new MapLoadOptions
    //    {
    //        Offset = new Vector2(2500f, 1500f)
    //    }))
    //    {
            //  We should figure out if it is possible to add this grid to the latejoin listing.
            //  Hey turns out we can! (This is kinda copypasted from the lodge with some values filled in.)
      //      if (_prototypeManager.TryIndex<GameMapPrototype>("Comet", out var stationProto))
      //      {
     //           _station.InitializeNewStation(stationProto.Stations["Comet"], cometUids);
     //       }
//
     //       var meta = EnsureComp<MetaDataComponent>(cometUids[0]);
      //      _meta.SetEntityName(cometUids[0], "NCWL Comet", meta);
      //      _shuttle.SetIFFColor(cometUids[0], civilianColor);
      //      _shuttle.SetIFFFaction(cometUids[0], "NCWL");
     //   }

     //    if (_map.TryLoad(mapId, aasim, out var famUids, new MapLoadOptions
     //        {
     //            Offset = new Vector2(4500f, 1500f)
     //        }))
     //     {
     //    //  We should figure out if it is possible to add this grid to the latejoin listing.
     //     // Hey turns out we can! (This is kinda copypasted from the lodge with some values filled in.)
     //        if (_prototypeManager.TryIndex<GameMapPrototype>("Aasim", out var stationProto))
     //        {
     //            _station.InitializeNewStation(stationProto.Stations["Aasim"], famUids);
     //       }
//
     //        var meta = EnsureComp<MetaDataComponent>(famUids[0]);
      //      _meta.SetEntityName(famUids[0], "TAP Qiwa Aasim", meta);
      //      _shuttle.SetIFFColor(famUids[0], civilianColor);
      //   }

           if (_map.TryLoad(mapId, tinnia, out var depotUid2s, new MapLoadOptions
            {
                Offset = tinniaOffset
           }))
         {
              var meta = EnsureComp<MetaDataComponent>(depotUid2s[0]);
              _meta.SetEntityName(depotUid2s[0], "Faint Signal", meta);
              _shuttle.SetIFFColor(depotUid2s[0], factionColor);
          }

        //   if (_map.TryLoad(mapId, church, out var churchUids, new MapLoadOptions
        //       {
        //           Offset = -tinniaOffset
        //       }))
        //   {
        //       var meta = EnsureComp<MetaDataComponent>(churchUids[0]);
        //       _meta.SetEntityName(churchUids[0], "Omnichurch Beacon", meta);
        //       _shuttle.SetIFFColor(churchUids[0], factionColor);
        //   }

        //   if (_map.TryLoad(mapId, lpbravo, out var lpbravoUids, new MapLoadOptions
        //        {
        //            Offset = _random.NextVector2(5150f, 4850f)
        //        }))
        //    {
        //        if (_prototypeManager.TryIndex<GameMapPrototype>("LPBravo", out var stationProto))
        //        {
        //           _station.InitializeNewStation(stationProto.Stations["LPBravo"], lpbravoUids);
        //       }
        //
        //      var meta = EnsureComp<MetaDataComponent>(lpbravoUids[0]);
        //      _meta.SetEntityName(lpbravoUids[0], "NCSP Grinning Jackal", meta);
        //     _shuttle.SetIFFColor(lpbravoUids[0], lpbravoColor);
        //     _shuttle.AddIFFFlag(lpbravoUids[0], IFFFlags.HideLabel);
        // }

        // if (_map.TryLoad(mapId, northpole, out var northpoleUids, new MapLoadOptions
        //     {
        //         Offset = _random.NextVector2(2150f, 3900f)
        //     }))
        // {
        //     var meta = EnsureComp<MetaDataComponent>(northpoleUids[0]);
        //     _shuttle.SetIFFColor(northpoleUids[0], lpbravoColor);
        //     _shuttle.AddIFFFlag(northpoleUids[0], IFFFlags.HideLabel);
        // }

            if (_map.TryLoad(mapId, arena, out var depotUid5s, new MapLoadOptions
              {
                  Offset = new Vector2(7200f, 5500f)
               }))
           {
               var meta = EnsureComp<MetaDataComponent>(depotUid5s[0]);
               _meta.SetEntityName(depotUid5s[0], "The Graveyard", meta);
               _shuttle.SetIFFColor(depotUid5s[0], lpbravoColor);
           }

           if (_map.TryLoad(mapId, stranded, out var depotUid20s, new MapLoadOptions
             {
                 Offset = new Vector2(7250f, 5320f)
             }))
         {
          var meta = EnsureComp<MetaDataComponent>(depotUid20s[0]);
           _meta.SetEntityName(depotUid20s[0], "Stranded Ship", meta);
          _shuttle.SetIFFColor(depotUid20s[0], lpbravoColor);
           _shuttle.AddIFFFlag(depotUid20s[0], IFFFlags.HideLabel);
         }

         if (_map.TryLoad(mapId, fighter1, out var depotUid21s, new MapLoadOptions
          {
             Offset = new Vector2(7730f, 5920f)
         }))
          {
           var meta = EnsureComp<MetaDataComponent>(depotUid21s[0]);
            _meta.SetEntityName(depotUid21s[0], "Destroyed Fighter", meta);
            _shuttle.SetIFFColor(depotUid21s[0], lpbravoColor);
           _shuttle.AddIFFFlag(depotUid21s[0], IFFFlags.HideLabel);
          }

        if (_map.TryLoad(mapId, fogwalker, out var depotfogs, new MapLoadOptions
        {
            Offset = new Vector2(1730f, 9920f)
        }))
        {
            var meta = EnsureComp<MetaDataComponent>(depotfogs[0]);
            _meta.SetEntityName(depotfogs[0], "Fogwrecked Derelict", meta);
            _shuttle.SetIFFColor(depotfogs[0], coveColor);
            _shuttle.AddIFFFlag(depotfogs[0], IFFFlags.HideLabel);
        }

        if (_map.TryLoad(mapId, fighter2, out var depotUid22s, new MapLoadOptions
         {
              Offset = new Vector2(7721f, 5950f)
         }))
          {
         var meta = EnsureComp<MetaDataComponent>(depotUid22s[0]);
         _meta.SetEntityName(depotUid22s[0], "Destroyed Fighter", meta);
          _shuttle.SetIFFColor(depotUid22s[0], lpbravoColor);
          _shuttle.AddIFFFlag(depotUid22s[0], IFFFlags.HideLabel);
          }

         if (_map.TryLoad(mapId, solarruined, out var depotUid23s, new MapLoadOptions
        {
             Offset = new Vector2(7750f, 5170f)
         }))
          {
            var meta = EnsureComp<MetaDataComponent>(depotUid23s[0]);
            _meta.SetEntityName(depotUid23s[0], "Solar Sailor Derelict", meta);
           _shuttle.SetIFFColor(depotUid23s[0], lpbravoColor);
            _shuttle.AddIFFFlag(depotUid23s[0], IFFFlags.HideLabel);
         }

         if (_map.TryLoad(mapId, impwreck, out var depotUid24s, new MapLoadOptions
         {
            Offset = new Vector2(7770f, 5750f)

         }))
         {
            var meta = EnsureComp<MetaDataComponent>(depotUid24s[0]);
           _meta.SetEntityName(depotUid24s[0], "Imperial Hauler Wreck", meta);
          _shuttle.SetIFFColor(depotUid24s[0], lpbravoColor);
           _shuttle.AddIFFFlag(depotUid24s[0], IFFFlags.HideLabel);
         }

        // if (_map.TryLoad(mapId, cove, out var depotUid6s, new MapLoadOptions
        //     {
        //         Offset = _random.NextVector2(10000f, 15000f)
        //     }))
        //{
        //    if (_prototypeManager.TryIndex<GameMapPrototype>("Cove", out var stationProto))
        //    {
        //        _station.InitializeNewStation(stationProto.Stations["Cove"], depotUid6s);
        //    }
        //
        //    var meta = EnsureComp<MetaDataComponent>(depotUid6s[0]);
        //    _meta.SetEntityName(depotUid6s[0], "DSM Countsman", meta);
        //    _shuttle.SetIFFColor(depotUid6s[0], coveColor);
        //    _shuttle.AddIFFFlag(depotUid6s[0], IFFFlags.HideLabel);
        //  }

        if (_map.TryLoad(mapId, hayes, out var depotUid7s, new MapLoadOptions
        {
             Offset = new Vector2(-3000, 6500f)
         }))
         {
             var meta = EnsureComp<MetaDataComponent>(depotUid7s[0]);
            _meta.SetEntityName(depotUid7s[0], "Derelict Waystation", meta);
            _shuttle.SetIFFColor(depotUid7s[0], lpbravoColor);
         }

        //   if (_map.TryLoad(mapId, lpramzi, out var depotUid8s, new MapLoadOptions
        //   {
        //      Offset = _random.NextVector2(8000f, 9000f)
        //  }))
        //  {
        //      if (_prototypeManager.TryIndex<GameMapPrototype>("lpramzi", out var stationProto))
        //      {
        //          _station.InitializeNewStation(stationProto.Stations["lpramzi"], depotUid8s);
        //     }
        //
        //     var meta = EnsureComp<MetaDataComponent>(depotUid8s[0]);
        //      _meta.SetEntityName(depotUid8s[0], "Listening Post 5", meta);
        //     _shuttle.SetIFFColor(depotUid8s[0], lpbravoColor);
        //     _shuttle.AddIFFFlag(depotUid8s[0], IFFFlags.HideLabel);
        // }

        //  if (_map.TryLoad(mapId, dochenskaya, out var depotUid9s, new MapLoadOptions
        //   {
        //       Offset = _random.NextVector2(9000f, 4000f)
        //   }))
        //   {
        //       if (_prototypeManager.TryIndex<GameMapPrototype>("Dochenskaya", out var stationProto))
        //      {
        //          _station.InitializeNewStation(stationProto.Stations["Dochenskaya"], depotUid9s);
        //     }
        //
        //     var meta = EnsureComp<MetaDataComponent>(depotUid9s[0]);
        //      _meta.SetEntityName(depotUid9s[0], "Dochenskaya Engineering Platform", meta);
        //      _shuttle.SetIFFColor(depotUid9s[0], lpbravoColor);
        //   }

          if (_map.TryLoad(mapId, jackal, out var depotUid9s, new MapLoadOptions
          {
              Offset = new Vector2(7794f, 4500f)
          }))
          {
             if (_prototypeManager.TryIndex<GameMapPrototype>("Jackal", out var stationProto))
            {
                _station.InitializeNewStation(stationProto.Stations["Jackal"], depotUid9s);
            }

            var meta = EnsureComp<MetaDataComponent>(depotUid9s[0]);
          _meta.SetEntityName(depotUid9s[0], "GSC Grinning Jackal", meta);
          _shuttle.SetIFFColor(depotUid9s[0], lpbravoColor);
          }

      //   if (_map.TryLoad(mapId, gliesssanto, out var depotUid92s, new MapLoadOptions
      //    {
      //       Offset = new Vector2(4200f, -4500f)
       //   }))
       //   {
       //     if (_prototypeManager.TryIndex<GameMapPrototype>("GliessSanto", out var stationProto))
       //     {
       //         _station.InitializeNewStation(stationProto.Stations["GliessSanto"], depotUid92s);
       //     }
//
       //      var meta = EnsureComp<MetaDataComponent>(depotUid92s[0]);
       //   _meta.SetEntityName(depotUid92s[0], "Gliess Santo", meta);
       //   _shuttle.SetIFFColor(depotUid92s[0], lpbravoColor);
      //    }

        //   if (_map.TryLoad(mapId, tatsumoto, out var depotUid10s, new MapLoadOptions
        //  {
        //      Offset = new Vector2(6000f, 1000f)
        //    }))
        //   {
        //       if (_prototypeManager.TryIndex<GameMapPrototype>("Tatsumoto", out var stationProto))
        //       {
        //            _station.InitializeNewStation(stationProto.Stations["Tatsumoto"], depotUid10s);
        //        }
        //
        //        var meta = EnsureComp<MetaDataComponent>(depotUid10s[0]);
        //        _meta.SetEntityName(depotUid10s[0], "Taypan Shipworks", meta);
        //        _shuttle.SetIFFColor(depotUid10s[0], tatsumotoColor);
        //        _shuttle.SetIFFFaction(depotUid10s[0], "SHI");
        //    }

          if (_map.TryLoad(mapId, oris, out var orisUids, new MapLoadOptions
          {
             Offset = new Vector2(3000f, 5400f)
          }))
          {
              var meta = EnsureComp<MetaDataComponent>(orisUids[0]);
              _meta.SetEntityName(orisUids[0], "Taypan-2 Asteroid Belt", meta);
              _shuttle.SetIFFColor(orisUids[0], factionColor);
         }

        //    if (_map.TryLoad(mapId, craster, out var crasterUids, new MapLoadOptions
        //   {
        //       Offset = new Vector2(-6500f, -12000f)
        // }))
        //  {
        //       var meta = EnsureComp<MetaDataComponent>(crasterUids[0]);
        //      _meta.SetEntityName(crasterUids[0], "Craster's Grave", meta);
        //      _shuttle.SetIFFColor(crasterUids[0], coveColor);
        //  }

          if (_map.TryLoad(mapId, taypanone, out var taypanoneUids, new MapLoadOptions
          {
               Offset = new Vector2(3000f, 3500f)
           }))
           {
               var meta = EnsureComp<MetaDataComponent>(taypanoneUids[0]);
              _meta.SetEntityName(taypanoneUids[0], "Taypan-1 Asteroid Belt", meta);
              _shuttle.SetIFFColor(taypanoneUids[0], factionColor);
          }

        //   if (_map.TryLoad(mapId, refuge, out var depotUid11s, new MapLoadOptions
        //    {
        //        Offset = _random.NextVector2(9000f, 12000f)
        //    }))
        //   {
        //     if (_prototypeManager.TryIndex<GameMapPrototype>("Refuge", out var stationProto))
        //       {
        //           _station.InitializeNewStation(stationProto.Stations["Refuge"], depotUid11s);
        //       }
        //
        //        var meta = EnsureComp<MetaDataComponent>(depotUid11s[0]);
        //       _meta.SetEntityName(depotUid11s[0], "The Refuge", meta);
        //       _shuttle.SetIFFColor(depotUid11s[0], refugeColor);
        //       _shuttle.AddIFFFlag(depotUid11s[0], IFFFlags.HideLabel);
        //    }

            if (_map.TryLoad(mapId, vladzena, out var depotUid12s, new MapLoadOptions
           {
               Offset = new Vector2(0f, 9000f)
           }))
           {
               if (_prototypeManager.TryIndex<GameMapPrototype>("Vladzena", out var stationProto))
               {
                  _station.InitializeNewStation(stationProto.Stations["Vladzena"], depotUid12s);
              }

             var meta = EnsureComp<MetaDataComponent>(depotUid12s[0]);
         _meta.SetEntityName(depotUid12s[0], "Port Vladzena", meta);
            _shuttle.SetIFFColor(depotUid12s[0], factionColor);
            _shuttle.SetIFFFaction(depotUid12s[0], "SHI");
         }

        //  if (_map.TryLoad(mapId, ardour, out var depotUid33s, new MapLoadOptions
        //  {
        //      Offset = new Vector2(0f, 9000f)
        //   }))S
        //   {
        //      if (_prototypeManager.TryIndex<GameMapPrototype>("Ardour", out var stationProto))
        //     {
        //         _station.InitializeNewStation(stationProto.Stations["Ardour"], depotUid33s);
        //      }
        //
        //       var meta = EnsureComp<MetaDataComponent>(depotUid33s[0]);
        //       _meta.SetEntityName(depotUid33s[0], "ATH Ardour 43BG-89", meta);
        //       _shuttle.SetIFFColor(depotUid33s[0], factionColor);
        //       _shuttle.SetIFFFaction(depotUid33s[0], "ATH");
        //   }

        // if (_map.TryLoad(mapId, lodge, out var lodgeUids, new MapLoadOptions
        //     {
        //         Offset = _random.NextVector2(1650f, 3400f)
        //     }))
        // {
        //     if (_prototypeManager.TryIndex<GameMapPrototype>("Lodge", out var stationProto))
        //     {
        //         _station.InitializeNewStation(stationProto.Stations["Lodge"], lodgeUids);
        //     }
        //
        //     var meta = EnsureComp<MetaDataComponent>(lodgeUids[0]);
        //     _meta.SetEntityName(lodgeUids[0], "Expeditionary Lodge", meta);
        //     _shuttle.SetIFFColor(lodgeUids[0], civilianColor);
        // }

        // if (_map.TryLoad(mapId, caseys, out var caseyUids, new MapLoadOptions
        //     {
        //         Offset = caseysOffset
        //    }))
        //{
        //     var meta = EnsureComp<MetaDataComponent>(caseyUids[0]);
        //     _meta.SetEntityName(caseyUids[0], "Crazy Casey's Casino", meta);
        //     _shuttle.SetIFFColor(caseyUids[0], factionColor);
        // }
        //
        // if (_map.TryLoad(mapId, grifty, out var griftyUids, new MapLoadOptions
        //     {
        //         Offset = -caseysOffset
        //    }))
        //{
        //    var meta = EnsureComp<MetaDataComponent>(griftyUids[0]);
        //   _meta.SetEntityName(griftyUids[0], "Grifty's Gas and Grub", meta);
        //    _shuttle.SetIFFColor(griftyUids[0], factionColor);
        // }

       //    if (_map.TryLoad(mapId, courthouse, out var depotUid8s, new MapLoadOptions()))
        //   {
       //        if (_prototypeManager.TryIndex<GameMapPrototype>("Kal", out var stationProto))
       //        {
       //            _station.InitializeNewStation(stationProto.Stations["Kal"], depotUid8s);
       //        }
       //       var meta = EnsureComp<MetaDataComponent>(depotUid8s[0]);
       //      _meta.SetEntityName(depotUid8s[0], "Kal Surezai", meta);
       //      _shuttle.SetIFFColor(depotUid8s[0], factionColor);
       //   }

        //  if (_map.TryLoad(mapId, lab, out var labUids, new MapLoadOptions
        //     {
        //         Offset = _random.NextVector2(2100f, 3800f)
        //     }))
        // {
        //     var meta = EnsureComp<MetaDataComponent>(labUids[0]);
        //     _meta.SetEntityName(labUids[0], "Derelict Research Outpost", meta);
        //     _shuttle.SetIFFColor(labUids[0], factionColor);
        //  }

        //if (_map.TryLoad(mapId, trade, out var tradeUids, new MapLoadOptions
        //{
        //    Offset = -tradeOffset
        //}))
        //{
        //    if (_prototypeManager.TryIndex<GameMapPrototype>("Trade", out var stationProto))
        //    {
        //        _station.InitializeNewStation(stationProto.Stations["Trade"], tradeUids);
        //    }
        //    var meta = EnsureComp<MetaDataComponent>(tradeUids[0]);
        //    _meta.SetEntityName(tradeUids[0], "Trade Outpost", meta);
        //    _shuttle.SetIFFColor(tradeUids[0], depotColor);
        //}

        //  var dungenTypes = _prototypeManager.EnumeratePrototypes<DungeonConfigPrototype>();
        //
        //   foreach (var dunGen in dungenTypes)
        //  {
        //
        //      var seed = _random.Next();
        //     var offset = _random.NextVector2(3000f, 8500f);
        //      if (!_map.TryLoad(mapId, "/Maps/spaceplatform.yml", out var grids, new MapLoadOptions
        //         {
        //             Offset = offset
        //         }))
        //     {
        //        continue;
        //    }
        //
        //       var mapGrid = EnsureComp<MapGridComponent>(grids[0]);
        //       _shuttle.AddIFFFlag(grids[0], IFFFlags.HideLabel);
        //      _console.WriteLine(null, $"dungeon spawned at {offset}");
        //      offset = new Vector2i(0, 0);

        //pls fit the grid I beg, this is so hacky
        //its better now but i think i need to do a normalization pass on the dungeon configs
        //because they are all offset. confirmed good size grid, just need to fix all the offsets.
        //     _dunGen.GenerateDungeon(dunGen, grids[0], mapGrid, (Vector2i) offset, seed);
        //   }
    }



}
