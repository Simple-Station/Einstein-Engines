using System.Linq;
using Content.Server.Players.PlayTimeTracking;
using Content.Shared._EE.Contractors.Prototypes;
using Content.Shared.CCVar;
using Content.Shared.Customization.Systems;
using Content.Shared.GameTicking;
using Content.Shared.Humanoid;
using Content.Shared.Players;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Shared;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Utility;

namespace Content.Server._EE.Contractors.Systems;

public sealed class EmployerSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly ISerializationManager _serialization = default!;
    [Dependency] private readonly CharacterRequirementsSystem _characterRequirements = default!;
    [Dependency] private readonly PlayTimeTrackingManager _playTimeTracking = default!;
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    [Dependency] private readonly IComponentFactory _componentFactory = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
    }

    // When the player is spawned in, add the employer components selected during character creation
    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent args) =>
        ApplyEmployer(args.Mob, args.JobId, args.Profile,
            _playTimeTracking.GetTrackerTimes(args.Player), args.Player.ContentData()?.Whitelisted ?? false);

    /// <summary>
    ///     Adds the employer selected by a player to an entity.
    /// </summary>
    public void ApplyEmployer(EntityUid uid, ProtoId<JobPrototype>? jobId, HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes, bool whitelisted)
    {
        if (jobId == null || !_prototype.TryIndex(jobId, out _))
            return;

        var jobPrototypeToUse = _prototype.Index(jobId.Value);

        ProtoId<EmployerPrototype> employer = profile.Employer != string.Empty ? profile.Employer : SharedHumanoidAppearanceSystem.DefaultEmployer;

        if(!_prototype.TryIndex<EmployerPrototype>(employer, out var employerPrototype))
        {
            DebugTools.Assert($"Employer '{employer}' not found!");
            return;
        }

        if (!_characterRequirements.CheckRequirementsValid(
            employerPrototype.Requirements,
            jobPrototypeToUse,
            profile, playTimes, whitelisted, employerPrototype,
            EntityManager, _prototype, _configuration,
            out _))
            return;

        AddEmployer(uid, employerPrototype);
    }

    /// <summary>
    ///     Adds a single Employer Prototype to an Entity.
    /// </summary>
    public void AddEmployer(EntityUid uid, EmployerPrototype employerPrototype)
    {
        if (!_configuration.GetCVar(CCVars.ContractorsEnabled) ||
            !_configuration.GetCVar(CCVars.ContractorsTraitFunctionsEnabled))
        {
            return;
        }

        foreach (var function in employerPrototype.Functions)
            function.OnPlayerSpawn(uid, _componentFactory, EntityManager, _serialization);
    }
}
