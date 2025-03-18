using System.Numerics;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Station.Systems;
using Content.Server.StationEvents.Components;
using Content.Shared.GameTicking.Components;
using Content.Shared.Humanoid;
using Content.Shared.Speech.Components;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Spawners;
using Robust.Shared.Timing;

namespace Content.Server.StationEvents.Events
{

    public sealed class BabelTowerRule : StationEventSystem<BabelTowerRuleComponent>
    {
        [Dependency] private readonly EntityLookupSystem _lookup = default!;

        private readonly HashSet<Entity<HumanoidAppearanceComponent>> _humans = new();

        // called when the gyatt is started
        protected override void Started(EntityUid uid, BabelTowerRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
        {
            base.Started(uid, component, gameRule, args);


            // get each and every speaking thing on the station's grid's map
            // we can only replace humanoids
            _lookup.GetEntitiesOnMap(GameTicker.DefaultMap, _humans);

            // add the Cogchamp accent to them
            foreach (var (hUid, _) in _humans)
            {
                try
                {
                    if (!EntityManager.HasComponent<RatvarianLanguageComponent>(hUid))
                    {
                        EntityManager.AddComponent<RatvarianLanguageComponent>(hUid);
                        Log.Debug("Hello fuck");
                    }
                } catch { }
            }

        }

        protected override void Ended(EntityUid uid, BabelTowerRuleComponent component, GameRuleComponent gameRule, GameRuleEndedEvent args)
        {
            // remove the Cogchamp accent to them
            foreach (var (hUid, _) in _humans)
            {
                try
                {
                    if (EntityManager.HasComponent<RatvarianLanguageComponent>(hUid))
                    {
                        EntityManager.RemoveComponent<RatvarianLanguageComponent>(hUid);
                        Log.Debug("Goodbye fuck");
                    }
                } catch { }
            }

        }
    }
}

