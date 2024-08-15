using Content.Server.Atmos.EntitySystems;
using Content.Server.Chat.Systems;
using Content.Server.Cloning.Components;
using Content.Server.DeviceLinking.Systems;
using Content.Server.EUI;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Humanoid;
using Content.Server.Jobs;
using Content.Server.Materials;
using Content.Server.Popups;
using Content.Server.Power.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.CCVar;
using Content.Shared.Chemistry.Components;
using Content.Shared.Cloning;
using Content.Shared.Damage;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.Emag.Components;
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
using Content.Server.Traits.Assorted;
using Content.Shared.Speech;
using Content.Shared.Tag;
using Content.Shared.Preferences;
using Content.Shared.Emoting;
using Content.Server.Speech.Components;
using Content.Server.StationEvents.Components;
using Content.Server.Ghost.Roles.Components;
using Content.Server.Nyanotrasen.Cloning;
using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.GameObjects.Components.Localization;
using Content.Shared.SSDIndicator;
using Content.Shared.Damage.ForceSay;
using Content.Shared.Chat;
using Content.Server.Body.Components;
using Content.Shared.Random.Helpers;
using Content.Shared.Contests;
using Content.Shared.Abilities.Psionics;
using Robust.Shared.Serialization.Manager;
using Content.Shared.Language.Components;
using Content.Shared.Language;
using Robust.Shared.Utility;
using Timer = Robust.Shared.Timing.Timer;
using Content.Server.Power.Components;
using Content.Shared.Damage.Prototypes;
using Content.Server.Database.Migrations.Postgres;

namespace Content.Server.Cloning
{
    public sealed class CloningSystem : EntitySystem
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
        }

        private void OnComponentInit(EntityUid uid, CloningPodComponent clonePod, ComponentInit args)
        {
            clonePod.BodyContainer = _containerSystem.EnsureContainer<ContainerSlot>(uid, "clonepod-bodyContainer");
            _signalSystem.EnsureSinkPorts(uid, CloningPodComponent.PodPort);
        }

        private void OnPowerChanged(EntityUid uid, CloningPodComponent component, PowerChangedEvent args)
        {
            if (!args.Powered
                && component.ActivelyCloning)
                CauseCloningFail(uid, component);
        }

        internal void TransferMindToClone(EntityUid mindId, MindComponent mind)
        {
            if (!ClonesWaitingForMind.TryGetValue(mind, out var entity)
                || !EntityManager.EntityExists(entity)
                || !TryComp<MindContainerComponent>(entity, out var mindComp)
                || mindComp.Mind != null)
                return;

            _mindSystem.TransferTo(mindId, entity, ghostCheckOverride: true, mind: mind);
            _mindSystem.UnVisit(mindId, mind);
            ClonesWaitingForMind.Remove(mind);
        }

        private void HandleMindAdded(EntityUid uid, BeingClonedComponent clonedComponent, MindAddedMessage message)
        {
            if (clonedComponent.Parent == EntityUid.Invalid
                || !EntityManager.EntityExists(clonedComponent.Parent)
                || !TryComp<CloningPodComponent>(clonedComponent.Parent, out var cloningPodComponent)
                || uid != cloningPodComponent.BodyContainer.ContainedEntity)
            {
                EntityManager.RemoveComponent<BeingClonedComponent>(uid);
                return;
            }
            UpdateStatus(clonedComponent.Parent, CloningPodStatus.Cloning, cloningPodComponent);
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

        private bool CheckUncloneable(EntityUid uid, EntityUid bodyToClone)
        {
            if (!TryComp<UncloneableComponent>(bodyToClone, out _))
                return true;

            _chatSystem.TrySendInGameICMessage(uid,
                Loc.GetString("cloning-console-uncloneable-trait-error"),
                InGameICChatType.Speak, false);

            return false;
        }

        private bool CheckBiomassCost(EntityUid uid, PhysicsComponent physics, CloningPodComponent clonePod)
        {
            var cloningCost = (int) Math.Round(physics.FixturesMass
                * _config.GetCVar(CCVars.CloningBiomassCostMultiplier)
                * clonePod.BiomassCostMultiplier);

            if (_material.GetMaterialAmount(uid, clonePod.RequiredMaterial) < cloningCost)
            {
                _chatSystem.TrySendInGameICMessage(uid, Loc.GetString("cloning-console-chat-error", ("units", cloningCost)), InGameICChatType.Speak, false);
                return false;
            }

            _material.TryChangeMaterialAmount(uid, clonePod.RequiredMaterial, -cloningCost);
            clonePod.UsedBiomass = cloningCost;

            return true;
        }

        private bool CheckGeneticDamage(EntityUid uid, EntityUid bodyToClone, CloningPodComponent clonePod, out float geneticDamage, float failChanceModifier = 1)
        {
            geneticDamage = 0;
            if (clonePod.DoMetempsychosis)
                return false;

            if (TryComp<DamageableComponent>(bodyToClone, out var damageable)
                && damageable.Damage.DamageDict.TryGetValue("Cellular", out var cellularDmg)
                && clonePod.ConnectedConsole is not null)
            {
                geneticDamage += (float) cellularDmg;
                var chance = Math.Clamp((float) (cellularDmg / 100), 0, 1);
                chance *= failChanceModifier;

                if (cellularDmg > 0)
                    _chatSystem.TrySendInGameICMessage(clonePod.ConnectedConsole.Value, Loc.GetString("cloning-console-cellular-warning", ("percent", Math.Round(100 - chance * 100))), InGameICChatType.Speak, false);

                if (_random.Prob(chance))
                {
                    CauseCloningFail(uid, clonePod);
                    return true;
                }
            }
            return false;
        }

        private void CauseCloningFail(EntityUid uid, CloningPodComponent component)
        {
            UpdateStatus(uid, CloningPodStatus.Gore, component);
            component.FailedClone = true;
            component.ActivelyCloning = true;
        }

        public bool TryCloning(EntityUid uid, EntityUid bodyToClone, Entity<MindComponent> mindEnt, CloningPodComponent clonePod, float failChanceModifier = 1)
        {
            if (!_mobStateSystem.IsDead(bodyToClone)
                || clonePod.ActivelyCloning
                || clonePod.ConnectedConsole == null
                || CheckUncloneable(uid, bodyToClone)
                || !TryComp<HumanoidAppearanceComponent>(bodyToClone, out var humanoid)
                || !TryComp<PhysicsComponent>(bodyToClone, out var physics)
                || CheckBiomassCost(uid, physics, clonePod)
                || !ClonesWaitingForMind.TryGetValue(mindEnt.Comp, out var clone)
                || !TryComp<MindContainerComponent>(clone, out var cloneMindComp)
                || cloneMindComp.Mind == null
                || cloneMindComp.Mind == mindEnt
                || mindEnt.Comp.UserId == null
                || !_playerManager.TryGetSessionById(mindEnt.Comp.UserId.Value, out var client))
                return false;

            ClonesWaitingForMind.Remove(mindEnt.Comp);

            var pref = humanoid.LastProfileLoaded;

            if (pref == null
                || !_prototypeManager.TryIndex(humanoid.Species, out var speciesPrototype))
                return false;

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

        public void AttemptCloning(EntityUid cloningPod, CloningPodComponent cloningPodComponent)
        {
            if (cloningPodComponent.BodyContainer.ContainedEntity is { Valid: true } entity
                && TryComp<PhysicsComponent>(entity, out var physics)
                && physics.Mass > 71)
                Timer.Spawn(TimeSpan.FromSeconds(cloningPodComponent.CloningTime * _contests.MassContest(entity, physics, true)), () => UpdateCloning(cloningPod, cloningPodComponent));

            Timer.Spawn(TimeSpan.FromSeconds(cloningPodComponent.CloningTime), () => UpdateCloning(cloningPod, cloningPodComponent));
        }

        public void UpdateCloning(EntityUid cloningPod, CloningPodComponent cloningPodComponent)
        {
            if (!cloningPodComponent.ActivelyCloning
                || !_powerReceiverSystem.IsPowered(cloningPod)
                || cloningPodComponent.BodyContainer.ContainedEntity == null
                || cloningPodComponent.FailedClone)
                EndFailedCloning(cloningPod, cloningPodComponent);

            Eject(cloningPod, cloningPodComponent);
        }

        public void UpdateStatus(EntityUid podUid, CloningPodStatus status, CloningPodComponent cloningPod)
        {
            cloningPod.Status = status;
            _appearance.SetData(podUid, CloningPodVisuals.Status, cloningPod.Status);
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

        public void Eject(EntityUid uid, CloningPodComponent? clonePod)
        {
            if (!Resolve(uid, ref clonePod)
                || clonePod.BodyContainer.ContainedEntity is not { Valid: true } entity
                || clonePod.CloningProgress < clonePod.CloningTime)
                return;

            EntityManager.RemoveComponent<BeingClonedComponent>(entity);
            _containerSystem.Remove(entity, clonePod.BodyContainer);
            clonePod.CloningProgress = 0f;
            clonePod.UsedBiomass = 0;
            UpdateStatus(uid, CloningPodStatus.Idle, clonePod);
            clonePod.ActivelyCloning = false;
        }

        private void EndFailedCloning(EntityUid uid, CloningPodComponent clonePod)
        {
            if (clonePod.BodyContainer.ContainedEntity is { Valid: true } entity)
            {
                if (TryComp<PhysicsComponent>(entity, out var physics)
                    && TryComp<BloodstreamComponent>(entity, out var bloodstream))
                    MakeAHugeMess(uid, physics, bloodstream);
                else MakeAHugeMess(uid);

                QueueDel(entity);
            }
            else MakeAHugeMess(uid);

            clonePod.FailedClone = false;
            clonePod.CloningProgress = 0f;
            UpdateStatus(uid, CloningPodStatus.Idle, clonePod);
            if (HasComp<EmaggedComponent>(uid))
            {
                _audio.PlayPvs(clonePod.ScreamSound, uid);
                Spawn(clonePod.MobSpawnId, Transform(uid).Coordinates);
            }

            if (!HasComp<EmaggedComponent>(uid))
                _material.SpawnMultipleFromMaterial(_random.Next(1, (int) (clonePod.UsedBiomass / 2.5)), clonePod.RequiredMaterial, Transform(uid).Coordinates);

            clonePod.UsedBiomass = 0;
            clonePod.ActivelyCloning = false;
        }

        /// <summary>
        ///     When failing to clone, much of the failed body is dissolved into a slurry of Ammonia and Blood, which spills from the machine.
        /// </summary>
        /// <remarks>
        ///     WOE BEFALLS WHOEVER FAILS TO CLONE A LAMIA
        /// </remarks>
        /// <param name="uid"></param>
        /// <param name="physics"></param>
        /// <param name="blood"></param>
        private void MakeAHugeMess(EntityUid uid, PhysicsComponent? physics = null, BloodstreamComponent? blood = null)
        {
            var tileMix = _atmosphereSystem.GetTileMixture(Transform(uid).GridUid, null, _transformSystem.GetGridTilePositionOrDefault((uid, Transform(uid))), true);
            Solution bloodSolution = new();

            tileMix?.AdjustMoles(Gas.Ammonia, 0.5f
                * ((physics is not null)
                ? physics.Mass
                : 71));

            bloodSolution.AddReagent("blood", 0.8f
                * ((blood is not null)
                ? blood.BloodMaxVolume
                : 300));

            _puddleSystem.TrySpillAt(uid, bloodSolution, out _);
        }

        /// <summary>
        ///     This function handles the Clone vs. Metem logic, as well as creation of the new body.
        /// </summary>
        private EntityUid FetchAndSpawnMob
        (EntityUid clonePod,
        CloningPodComponent clonePodComp,
        HumanoidCharacterProfile pref,
        SpeciesPrototype speciesPrototype,
        HumanoidAppearanceComponent humanoid,
        EntityUid bodyToClone,
        float geneticDamage)
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

            // Put the clone in crit with high Cellular damage. Medbay should use Cryogenics to "Finish" clones. Doxarubixadone is perfect for this.
            if (HasComp<DamageableComponent>(mob))
            {
                DamageSpecifier damage = new(_prototypeManager.Index<DamageGroupPrototype>("Cellular"), 101f + geneticDamage);
                _damageable.TryChangeDamage(mob, damage, true);
            }

            if (TryComp<HumanoidAppearanceComponent>(mob, out var newHumanoid))
            {
                if (switchingSpecies && !forceOldProfile)
                {
                    var flavorText = _serialization.CreateCopy(pref.FlavorText, null, false, true);
                    var oldName = _serialization.CreateCopy(pref.Name, null, false, true);

                    pref = HumanoidCharacterProfile.RandomWithSpecies(newHumanoid.Species);

                    if (sexes.Contains(humanoid.Sex)
                        && _config.GetCVar(CCVars.CloningPreserveSex))
                        pref = pref.WithSex(humanoid.Sex);

                    if (_config.GetCVar(CCVars.CloningPreserveGender))
                        pref = pref.WithGender(humanoid.Gender);
                    else oldGender = humanoid.Gender;

                    if (_config.GetCVar(CCVars.CloningPreserveAge))
                        pref = pref.WithAge(humanoid.Age);

                    if (_config.GetCVar(CCVars.CloningPreserveHeight))
                        pref = pref.WithHeight(humanoid.Height);

                    if (_config.GetCVar(CCVars.CloningPreserveWidth))
                        pref = pref.WithWidth(humanoid.Width);

                    if (_config.GetCVar(CCVars.CloningPreserveName))
                        pref = pref.WithName(oldName);

                    if (_config.GetCVar(CCVars.CloningPreserveFlavorText))
                        pref = pref.WithFlavorText(flavorText);
                }
                _humanoidSystem.LoadProfile(mob, pref);
            }

            var ev = new CloningEvent(bodyToClone, mob);
            RaiseLocalEvent(bodyToClone, ref ev);

            if (!ev.NameHandled)
                _metaSystem.SetEntityName(mob, MetaData(bodyToClone).EntityName);

            var grammar = EnsureComp<GrammarComponent>(mob);
            grammar.ProperNoun = true;
            grammar.Gender = oldGender;
            Dirty(mob, grammar);

            if (forceOldProfile
                && TryComp<PsionicComponent>(bodyToClone, out var psionic))
            {
                var newPsionic = _serialization.CreateCopy(psionic, null, false, true);
                AddComp(mob, newPsionic, true);
            }

            if (TryComp<LanguageKnowledgeComponent>(bodyToClone, out var oldKnowLangs))
            {
                var newKnowLangs = _serialization.CreateCopy(oldKnowLangs, null, false, true);
                AddComp(mob, newKnowLangs, true);
            }


            if (TryComp<LanguageSpeakerComponent>(bodyToClone, out var oldSpeakLangs))
            {
                var newSpeakLangs = _serialization.CreateCopy(oldSpeakLangs, null, false, true);
                AddComp(mob, newSpeakLangs, true);
            }


            EnsureComp<SpeechComponent>(mob);
            EnsureComp<DamageForceSayComponent>(mob);
            EnsureComp<EmotingComponent>(mob);
            EnsureComp<MindContainerComponent>(mob);
            EnsureComp<SSDIndicatorComponent>(mob);
            RemComp<ReplacementAccentComponent>(mob);
            RemComp<MonkeyAccentComponent>(mob);
            RemComp<SentienceTargetComponent>(mob);
            RemComp<GhostTakeoverAvailableComponent>(mob);

            _tag.AddTag(mob, "DoorBumpOpener");

            return mob;
        }
        public void Reset(RoundRestartCleanupEvent ev)
        {
            ClonesWaitingForMind.Clear();
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
}
