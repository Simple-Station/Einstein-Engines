using Content.Server.Atmos.EntitySystems;
using Content.Server.Chat.Systems;
using Content.Server.Cloning.Components;
using Content.Server.Construction;
using Content.Server.DeviceLinking.Systems;
using Content.Server.EUI;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Humanoid;
using Content.Server.Jobs;
using Content.Server.Materials;
using Content.Server.Popups;
using Content.Server.Power.EntitySystems;
using Content.Shared.Silicon.Components; // Goobstation
using Content.Shared.Atmos;
using Content.Shared.CCVar;
using Content.Shared.Chemistry.Components;
using Content.Shared.Cloning;
using Content.Shared.Damage;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.Emag.Systems;
using Content.Shared.Examine;
using Content.Shared.GameTicking;
using Content.Shared.Humanoid;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Random;
using Content.Shared.Roles.Jobs;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Containers;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Content.Shared.Tag;
using Content.Shared.Preferences;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Random.Helpers;
using Content.Shared.Contests;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Utility;
using Timer = Robust.Shared.Timing.Timer;
using Content.Server.Power.Components;
using Content.Shared.Drunk;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Power;


namespace Content.Server.Cloning;

public sealed partial class CloningSystem : EntitySystem
{
    [Dependency] private readonly DeviceLinkSystem _signalSystem = default!;
    [Dependency] private readonly IPlayerManager _playerManager = null!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly EuiManager _euiManager = null!;
    [Dependency] private readonly CloningConsoleSystem _cloningConsoleSystem = default!;
    [Dependency] private readonly HumanoidAppearanceSystem _humanoidSystem = default!;
    [Dependency] private readonly ContainerSystem _containerSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly PowerReceiverSystem _powerReceiverSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
    [Dependency] private readonly TransformSystem _transformSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly PuddleSystem _puddleSystem = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly MaterialStorageSystem _material = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedMindSystem _mindSystem = default!;
    [Dependency] private readonly MetaDataSystem _metaSystem = default!;
    [Dependency] private readonly SharedJobSystem _jobs = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly ContestsSystem _contests = default!;
    [Dependency] private readonly ISerializationManager _serialization = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly ThirstSystem _thirst = default!;
    [Dependency] private readonly SharedDrunkSystem _drunk = default!;
    [Dependency] private readonly MobThresholdSystem _thresholds = default!;
    public readonly Dictionary<MindComponent, EntityUid> ClonesWaitingForMind = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CloningPodComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(Reset);
        SubscribeLocalEvent<BeingClonedComponent, MindAddedMessage>(HandleMindAdded);
        SubscribeLocalEvent<CloningPodComponent, PortDisconnectedEvent>(OnPortDisconnected);
        SubscribeLocalEvent<CloningPodComponent, AnchorStateChangedEvent>(OnAnchor);
        SubscribeLocalEvent<CloningPodComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<CloningPodComponent, GotEmaggedEvent>(OnEmagged);
        SubscribeLocalEvent<CloningPodComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<CloningPodComponent, RefreshPartsEvent>(OnPartsRefreshed);
        SubscribeLocalEvent<CloningPodComponent, UpgradeExamineEvent>(OnUpgradeExamine);
    }
    private void OnPartsRefreshed(EntityUid uid, CloningPodComponent component, RefreshPartsEvent args)
    {
        var materialRating = args.PartRatings[component.MachinePartMaterialUse];
        var speedRating = args.PartRatings[component.MachinePartCloningSpeed];

        component.BiomassCostMultiplier = MathF.Pow(component.PartRatingMaterialMultiplier, materialRating - 1);
        component.CloningTime = component.CloningTime * MathF.Pow(component.PartRatingSpeedMultiplier, speedRating - 1);
    }

    private void OnUpgradeExamine(EntityUid uid, CloningPodComponent component, UpgradeExamineEvent args)
    {
        args.AddPercentageUpgrade("cloning-pod-component-upgrade-speed", component.CloningTime / component.CloningTime);
        args.AddPercentageUpgrade("cloning-pod-component-upgrade-biomass-requirement", component.BiomassCostMultiplier);
    }
    private void OnPortDisconnected(EntityUid uid, CloningPodComponent pod, PortDisconnectedEvent args)
    {
        pod.ConnectedConsole = null;
    }

    private void OnAnchor(EntityUid uid, CloningPodComponent component, ref AnchorStateChangedEvent args)
    {
        if (component.ActivelyCloning)
            CauseCloningFail(uid, component);

        if (component.ConnectedConsole == null
            || !TryComp<CloningConsoleComponent>(component.ConnectedConsole, out var console)
            || !args.Anchored
            || !_cloningConsoleSystem.RecheckConnections(component.ConnectedConsole.Value, uid, console.GeneticScanner, console))
            return;

        _cloningConsoleSystem.UpdateUserInterface(component.ConnectedConsole.Value, console);
    }

    private void OnExamined(EntityUid uid, CloningPodComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange
            || !_powerReceiverSystem.IsPowered(uid))
            return;

        args.PushMarkup(Loc.GetString("cloning-pod-biomass", ("number", _material.GetMaterialAmount(uid, component.RequiredMaterial))));
    }
    private void OnComponentInit(EntityUid uid, CloningPodComponent clonePod, ComponentInit args)
    {
        clonePod.BodyContainer = _containerSystem.EnsureContainer<ContainerSlot>(uid, "clonepod-bodyContainer");
        _signalSystem.EnsureSinkPorts(uid, CloningPodComponent.PodPort);
    }

    private void OnPowerChanged(EntityUid uid, CloningPodComponent component, PowerChangedEvent args)
    {
        if (!args.Powered && component.ActivelyCloning)
            CauseCloningFail(uid, component);
    }

    /// <summary>
    ///     On emag, spawns a failed clone when cloning process fails which attacks nearby crew.
    /// </summary>
    private void OnEmagged(EntityUid uid, CloningPodComponent clonePod, ref GotEmaggedEvent args)
    {
        if (!this.IsPowered(uid, EntityManager))
            return;

        if (clonePod.ActivelyCloning)
            CauseCloningFail(uid, clonePod);

        _audio.PlayPvs(clonePod.SparkSound, uid);
        _popupSystem.PopupEntity(Loc.GetString("cloning-pod-component-upgrade-emag-requirement"), uid);
        args.Handled = true;
    }

    private void Reset(RoundRestartCleanupEvent ev)
    {
        ClonesWaitingForMind.Clear();
    }

    /// <summary>
    ///     The master function behind Cloning, called by the cloning console via button press to start the cloning process.
    /// </summary>
    public bool TryCloning(EntityUid uid, EntityUid bodyToClone, Entity<MindComponent> mindEnt, CloningPodComponent clonePod, float failChanceModifier = 1)
    {
        if (!_mobStateSystem.IsDead(bodyToClone)
            || clonePod.ActivelyCloning
            || clonePod.ConnectedConsole == null
            || !CheckUncloneable(uid, bodyToClone, clonePod, out var cloningCostMultiplier)
            || !TryComp<HumanoidAppearanceComponent>(bodyToClone, out var humanoid)
            || !TryComp<PhysicsComponent>(bodyToClone, out var physics))
            return false;

        var mind = mindEnt.Comp;
        if (ClonesWaitingForMind.TryGetValue(mind, out var clone))
        {
            if (EntityManager.EntityExists(clone) &&
                !_mobStateSystem.IsDead(clone) &&
                TryComp<MindContainerComponent>(clone, out var cloneMindComp) &&
                (cloneMindComp.Mind == null || cloneMindComp.Mind == mindEnt))
                return false; // Mind already has clone

            ClonesWaitingForMind.Remove(mind);
        }

        if (mind.OwnedEntity != null && !_mobStateSystem.IsDead(mind.OwnedEntity.Value)
            || mind.UserId == null
            || !_playerManager.TryGetSessionById(mind.UserId.Value, out var client)
            || !CheckBiomassCost(uid, physics, clonePod, cloningCostMultiplier))
            return false;
        
        // Special handling for humanoid data related to metempsychosis. This function is needed for Paradox Anomaly code to play nice with reincarnated people
        var pref = humanoid.LastProfileLoaded;
        if (pref == null
            || !_prototypeManager.TryIndex(humanoid.Species, out var speciesPrototype))
            return false;

        if (HasComp<SiliconComponent>(bodyToClone))
            return false; // Goobstation: Don't clone IPCs.

        // Yes, this can return true without making a body. If it returns true, we're making clone soup instead.
        if (CheckGeneticDamage(uid, bodyToClone, clonePod, out var geneticDamage, failChanceModifier))
            return true;

        var mob = FetchAndSpawnMob(uid, clonePod, pref, speciesPrototype, humanoid, bodyToClone, geneticDamage);
        var ev = new CloningEvent(bodyToClone, mob);
        RaiseLocalEvent(bodyToClone, ref ev);

        if (!ev.NameHandled)
            _metaSystem.SetEntityName(mob, MetaData(bodyToClone).EntityName);

        var cloneMindReturn = EntityManager.AddComponent<BeingClonedComponent>(mob);
        cloneMindReturn.Mind = mindEnt.Comp;
        cloneMindReturn.Parent = uid;
        _containerSystem.Insert(mob, clonePod.BodyContainer);
        ClonesWaitingForMind.Add(mindEnt.Comp, mob);
        UpdateStatus(uid, CloningPodStatus.NoMind, clonePod);
        _euiManager.OpenEui(new AcceptCloningEui(mindEnt, mindEnt.Comp, this), client);

        clonePod.ActivelyCloning = true;

        if (_jobs.MindTryGetJob(mindEnt, out _, out var prototype))
            foreach (var special in prototype.Special)
                if (special is AddComponentSpecial)
                    special.AfterEquip(mob);

        return true;
    }

    /// <summary>
    ///     Begins the cloning timer, which at the end can either produce clone soup, or a functional body, depending on if anything interrupts the procedure.
    /// </summary>
    public void AttemptCloning(EntityUid cloningPod, CloningPodComponent cloningPodComponent)
    {
        if (cloningPodComponent.BodyContainer.ContainedEntity is { Valid: true } entity
            && TryComp<PhysicsComponent>(entity, out var physics)
            && physics.Mass > 71)
            Timer.Spawn(TimeSpan.FromSeconds(cloningPodComponent.CloningTime * _contests.MassContest(entity, physics, true)), () => EndCloning(cloningPod, cloningPodComponent));

        Timer.Spawn(TimeSpan.FromSeconds(cloningPodComponent.CloningTime), () => EndCloning(cloningPod, cloningPodComponent));
    }

    /// <summary>
    ///     Ding, your body is ready. Time to find out if it's soup or solid.
    /// </summary>
    public void EndCloning(EntityUid cloningPod, CloningPodComponent cloningPodComponent)
    {
        if (!cloningPodComponent.ActivelyCloning
            || !_powerReceiverSystem.IsPowered(cloningPod)
            || cloningPodComponent.BodyContainer.ContainedEntity == null
            || cloningPodComponent.FailedClone)
            EndFailedCloning(cloningPod, cloningPodComponent); //Surprise, it's soup!

        Eject(cloningPod, cloningPodComponent); //Hey look, a body!
    }

    public void UpdateStatus(EntityUid podUid, CloningPodStatus status, CloningPodComponent cloningPod)
    {
        cloningPod.Status = status;
        _appearance.SetData(podUid, CloningPodVisuals.Status, cloningPod.Status);
    }

    /// <summary>
    ///     This function handles the Clone vs. Metem logic, as well as creation of the new body.
    /// </summary>
    private EntityUid FetchAndSpawnMob(
        EntityUid clonePod,
        CloningPodComponent clonePodComp,
        HumanoidCharacterProfile pref,
        SpeciesPrototype speciesPrototype,
        HumanoidAppearanceComponent humanoid,
        EntityUid bodyToClone,
        float geneticDamage
    )
    {
        List<Sex> sexes = new();
        bool switchingSpecies = false;
        var toSpawn = speciesPrototype.Prototype;
        var forceOldProfile = true;
        var oldKarma = 0;
        var oldGender = humanoid.Gender;
        if (TryComp<MetempsychosisKarmaComponent>(bodyToClone, out var oldKarmaComp))
            oldKarma += oldKarmaComp.Score;

        if (clonePodComp.DoMetempsychosis)
        {
            toSpawn = GetSpawnEntity(bodyToClone, clonePodComp, speciesPrototype, oldKarma, out var newSpecies, out var changeProfile);
            forceOldProfile = !changeProfile;
            oldKarma++;

            if (changeProfile)
                geneticDamage = 0;

            if (newSpecies != null)
            {
                sexes = newSpecies.Sexes;

                if (speciesPrototype.ID != newSpecies.ID)
                    switchingSpecies = true;
            }
        }
        EntityUid mob = Spawn(toSpawn, _transformSystem.GetMapCoordinates(clonePod));
        EnsureComp<MetempsychosisKarmaComponent>(mob, out var newKarma);
        newKarma.Score += oldKarma;

        UpdateCloneDamage(mob, clonePodComp, geneticDamage);
        UpdateCloneAppearance(mob, pref, humanoid, sexes, oldGender, switchingSpecies, forceOldProfile, out var gender);
        var ev = new CloningEvent(bodyToClone, mob);
        RaiseLocalEvent(bodyToClone, ref ev);

        if (!ev.NameHandled)
            _metaSystem.SetEntityName(mob, MetaData(bodyToClone).EntityName);

        UpdateGrammar(mob, gender);
        CleanupCloneComponents(mob, bodyToClone, forceOldProfile, clonePodComp.DoMetempsychosis);
        UpdateHungerAndThirst(mob, clonePodComp);

        return mob;
    }

    public string GetSpawnEntity(EntityUid oldBody, CloningPodComponent component, SpeciesPrototype oldSpecies, int karma, out SpeciesPrototype? species, out bool changeProfile)
    {
        changeProfile = true;
        species = oldSpecies;
        if (!_prototypeManager.TryIndex<WeightedRandomPrototype>(component.MetempsychoticHumanoidPool, out var humanoidPool)
            || !_prototypeManager.TryIndex<SpeciesPrototype>(humanoidPool.Pick(), out var speciesPrototype)
            || !_prototypeManager.TryIndex<WeightedRandomPrototype>(component.MetempsychoticNonHumanoidPool, out var nonHumanoidPool)
            || !_prototypeManager.TryIndex<EntityPrototype>(nonHumanoidPool.Pick(), out var entityPrototype))
        {
            DebugTools.Assert("Could not index species for metempsychotic machine.");
            changeProfile = false;
            return oldSpecies.Prototype;
        }
        var chance = (component.HumanoidBaseChance - karma * component.KarmaOffset) * _contests.MindContest(oldBody, true);


        var ev = new ReincarnatingEvent(oldBody, chance);
        RaiseLocalEvent(oldBody, ref ev);

        chance = ev.OverrideChance
            ? ev.ReincarnationChances
            : chance * ev.ReincarnationChanceModifier;

        switch (ev.ForcedType)
        {
            case ForcedMetempsychosisType.None:
                if (!ev.NeverTrulyClone
                    && chance > 1
                    && _random.Prob(chance - 1))
                {
                    changeProfile = false;
                    return oldSpecies.Prototype;
                }

                chance = Math.Clamp(chance, 0, 1);
                if (_random.Prob(chance))
                {
                    species = speciesPrototype;
                    return speciesPrototype.Prototype;
                }
                species = null;
                return entityPrototype.ID;

            case ForcedMetempsychosisType.Clone:
                changeProfile = false;
                return oldSpecies.Prototype;

            case ForcedMetempsychosisType.RandomHumanoid:
                species = speciesPrototype;
                return speciesPrototype.Prototype;

            case ForcedMetempsychosisType.RandomNonHumanoid:
                species = null;
                return entityPrototype.ID;
        }
        changeProfile = false;
        return oldSpecies.Prototype;
    }
}
