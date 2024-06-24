using System.Linq;
using Content.Server.GameTicking;
using Content.Server.Players.PlayTimeTracking;
using Content.Shared.Customization.Systems;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Roles;
using Content.Shared.Traits;
using Pidgin.Configuration;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;

namespace Content.Server.Traits;

public sealed class TraitSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly ISerializationManager _serialization = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly CharacterRequirementsSystem _characterRequirements = default!;
    [Dependency] private readonly PlayTimeTrackingManager _playTimeTracking = default!;
    [Dependency] private readonly IConfigurationManager _configuration = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
    }

    // When the player is spawned in, add all trait components selected during character creation
    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent args)
    {
        foreach (var traitId in args.Profile.TraitPreferences)
        {
            if (!_prototype.TryIndex<TraitPrototype>(traitId, out var traitPrototype))
            {
                Log.Warning($"No trait found with ID {traitId}!");
                return;
            }

            if (!_characterRequirements.CheckRequirementsValid(traitPrototype, traitPrototype.Requirements,
                _prototype.Index<JobPrototype>(args.JobId ?? _prototype.EnumeratePrototypes<JobPrototype>().First().ID),
                args.Profile, _playTimeTracking.GetTrackerTimes(args.Player),
                EntityManager, _prototype, _configuration,
                out _))
                continue;

            // Add all components required by the prototype
            foreach (var entry in traitPrototype.Components.Values)
            {
                if (HasComp(args.Mob, entry.Component.GetType()))
                    continue;

                var comp = (Component) _serialization.CreateCopy(entry.Component, notNullableOverride: true);
                comp.Owner = args.Mob;
                EntityManager.AddComponent(args.Mob, comp);
            }
        }
    }
}
