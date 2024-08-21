using System.Linq;
using Content.Shared.Actions;
using Content.Server.GameTicking;
using Content.Server.Players.PlayTimeTracking;
using Content.Shared.Customization.Systems;
using Content.Shared.Players;
using Content.Shared.Roles;
using Content.Shared.Traits;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;
using Content.Server.Abilities.Psionics;

namespace Content.Server.Traits;

public sealed class TraitSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly ISerializationManager _serialization = default!;
    [Dependency] private readonly CharacterRequirementsSystem _characterRequirements = default!;
    [Dependency] private readonly PlayTimeTrackingManager _playTimeTracking = default!;
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly PsionicAbilitiesSystem _psionicAbilities = default!;

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

            if (!_characterRequirements.CheckRequirementsValid(
                traitPrototype.Requirements,
                _prototype.Index<JobPrototype>(args.JobId ?? _prototype.EnumeratePrototypes<JobPrototype>().First().ID),
                args.Profile, _playTimeTracking.GetTrackerTimes(args.Player), args.Player.ContentData()?.Whitelisted ?? false,
                EntityManager, _prototype, _configuration,
                out _))
                continue;

            if (traitPrototype.Components is not null)
                AddTraitComponents(args.Mob, traitPrototype);

            if (traitPrototype.Actions is not null)
                AddTraitActions(args.Mob, traitPrototype);

            if (traitPrototype.PsionicPowers is not null)
                AddTraitPsionics(args.Mob, traitPrototype);
        }
    }

    /// <summary>
    ///     Adds all Components included with a Trait.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="traitPrototype"></param>
    private void AddTraitComponents(EntityUid uid, TraitPrototype traitPrototype)
    {
        foreach (var entry in traitPrototype.Components.Values)
        {
            if (HasComp(uid, entry.Component.GetType()))
                continue;

            var comp = (Component) _serialization.CreateCopy(entry.Component, notNullableOverride: true);
            comp.Owner = uid;
            EntityManager.AddComponent(uid, comp);
        }
    }

    /// <summary>
    ///     Add all actions associated with a specific Trait
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="proto"></param>
    /// <param name="psionic"></param>
    private void AddTraitActions(EntityUid uid, TraitPrototype traitPrototype)
    {
        foreach (var id in traitPrototype.Actions)
        {
            EntityUid? actionId = null;
            if (_actions.AddAction(uid, ref actionId, id))
            {
                _actions.StartUseDelay(actionId);
            }
        }
    }

    /// <summary>
    ///     If a trait includes any Psionic Powers, this enters the powers into PsionicSystem to be initialized.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="traitPrototype"></param>
    private void AddTraitPsionics(EntityUid uid, TraitPrototype traitPrototype)
    {
        foreach (var powerProto in traitPrototype.PsionicPowers)
            _psionicAbilities.InitializePsionicPower(uid, powerProto, false);
    }
}
