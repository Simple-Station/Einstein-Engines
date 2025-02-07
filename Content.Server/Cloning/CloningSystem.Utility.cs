using Content.Server.Cloning.Components;
using Content.Shared.Atmos;
using Content.Shared.CCVar;
using Content.Shared.Chemistry.Components;
using Content.Shared.Cloning;
using Content.Shared.Damage;
using Content.Shared.Emag.Components;
using Content.Shared.Humanoid;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Robust.Shared.Physics.Components;
using Robust.Shared.Random;
using Content.Shared.Speech;
using Content.Shared.Preferences;
using Content.Shared.Emoting;
using Content.Server.Speech.Components;
using Content.Server.StationEvents.Components;
using Content.Server.Ghost.Roles.Components;
using Robust.Shared.GameObjects.Components.Localization;
using Content.Shared.SSDIndicator;
using Content.Shared.Damage.ForceSay;
using Content.Shared.Chat;
using Content.Server.Body.Components;
using Content.Server.Language;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Language.Components;
using Content.Shared.Nutrition.Components;
using Robust.Shared.Enums;

namespace Content.Server.Cloning;

public sealed partial class CloningSystem
{
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

    /// <summary>
    ///     Test if the body to be cloned has any conditions that would prevent cloning from taking place.
    ///     Or, if the body has a particular reason to make cloning more difficult.
    /// </summary>
    private bool CheckUncloneable(EntityUid uid, EntityUid bodyToClone, CloningPodComponent clonePod, out float cloningCostMultiplier)
    {
        var ev = new AttemptCloningEvent(uid, clonePod.DoMetempsychosis);
        RaiseLocalEvent(bodyToClone, ref ev);
        cloningCostMultiplier = ev.CloningCostMultiplier;

        if (ev.Cancelled && ev.CloningFailMessage is not null)
        {
            if (clonePod.ConnectedConsole is not null)
                _chatSystem.TrySendInGameICMessage(clonePod.ConnectedConsole.Value,
                    Loc.GetString(ev.CloningFailMessage),
                    InGameICChatType.Speak, false);
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Checks the body's physics component and any previously obtained modifiers to determine biomass cost.
    ///     If there is insufficient biomass, the cloning cannot start.
    /// </summary>
    private bool CheckBiomassCost(EntityUid uid, PhysicsComponent physics, CloningPodComponent clonePod, float cloningCostMultiplier = 1)
    {
        if (clonePod.ConnectedConsole is null)
            return false;

        var cloningCost = (int) Math.Round(physics.FixturesMass
            * _config.GetCVar(CCVars.CloningBiomassCostMultiplier)
            * clonePod.BiomassCostMultiplier
            * cloningCostMultiplier);

        if (_material.GetMaterialAmount(uid, clonePod.RequiredMaterial) < cloningCost)
        {
            _chatSystem.TrySendInGameICMessage(clonePod.ConnectedConsole.Value, Loc.GetString("cloning-console-chat-error", ("units", cloningCost)), InGameICChatType.Speak, false);
            return false;
        }

        _material.TryChangeMaterialAmount(uid, clonePod.RequiredMaterial, -cloningCost);
        clonePod.UsedBiomass = cloningCost;

        return true;
    }

    /// <summary>
    ///     Tests the original body for genetic damage, while returning the cloning damage for later damage.
    ///     The body's cellular damage is also used as a potential failure state, giving a chance for the cloning to fail immediately.
    /// </summary>
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

    /// <summary>
    ///     When this condition is called, it sets the cloning pod to its fail condition.
    ///     Such that when the cloning timer ends, the body that would be created, is turned into clone soup.
    /// </summary>
    private void CauseCloningFail(EntityUid uid, CloningPodComponent component)
    {
        UpdateStatus(uid, CloningPodStatus.Gore, component);
        component.FailedClone = true;
        component.ActivelyCloning = true;
    }

    /// <summary>
    ///     This is the success condition for cloning. At the end of the timer, if nothing interrupted it, this function is called to finish the cloning by dispensing the body.
    /// </summary>
    private void Eject(EntityUid uid, CloningPodComponent? clonePod)
    {
        if (!Resolve(uid, ref clonePod)
            || clonePod.BodyContainer.ContainedEntity is null)
            return;

        var entity = clonePod.BodyContainer.ContainedEntity.Value;
        EntityManager.RemoveComponent<BeingClonedComponent>(entity);
        _containerSystem.Remove(entity, clonePod.BodyContainer);
        clonePod.CloningProgress = 0f;
        clonePod.UsedBiomass = 0;
        UpdateStatus(uid, CloningPodStatus.Idle, clonePod);
        clonePod.ActivelyCloning = false;
    }

    /// <summary>
    ///     And now we turn it over to Chef Pod to make soup!
    /// </summary>
    private void EndFailedCloning(EntityUid uid, CloningPodComponent clonePod)
    {
        if (clonePod.BodyContainer.ContainedEntity is not null)
        {
            var entity = clonePod.BodyContainer.ContainedEntity.Value;
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
            _material.SpawnMultipleFromMaterial(_random.Next(1, Math.Max(1, (int) (clonePod.UsedBiomass / 2.5))), clonePod.RequiredMaterial, Transform(uid).Coordinates);

        clonePod.UsedBiomass = 0;
        clonePod.ActivelyCloning = false;
    }

    /// <summary>
    ///     The body coming out of the machine isn't guaranteed to even be a Humanoid.
    ///     This function makes sure the body is "Human Playable", with no funny business.
    /// </summary>
    private void CleanupCloneComponents(EntityUid uid, EntityUid bodyToClone, bool forceOldProfile, bool doMetempsychosis)
    {
        if (forceOldProfile
            && TryComp<PsionicComponent>(bodyToClone, out var psionic))
        {
            var newPsionic = _serialization.CreateCopy(psionic, null, false, true);
            AddComp(uid, newPsionic, true);
        }

        if (TryComp<LanguageKnowledgeComponent>(bodyToClone, out var oldKnowLangs))
        {
            var newKnowLangs = _serialization.CreateCopy(oldKnowLangs, null, false, true);
            AddComp(uid, newKnowLangs, true);
        }


        if (TryComp<LanguageSpeakerComponent>(bodyToClone, out var oldSpeakLangs))
        {
            var newSpeakLangs = _serialization.CreateCopy(oldSpeakLangs, null, false, true);
            AddComp(uid, newSpeakLangs, true);
        }

        if (doMetempsychosis)
            EnsureComp<PsionicComponent>(uid);

        EnsureComp<SpeechComponent>(uid);
        EnsureComp<DamageForceSayComponent>(uid);
        EnsureComp<EmotingComponent>(uid);
        EnsureComp<MindContainerComponent>(uid);
        EnsureComp<SSDIndicatorComponent>(uid);
        RemComp<ReplacementAccentComponent>(uid);
        RemComp<MonkeyAccentComponent>(uid);
        RemComp<SentienceTargetComponent>(uid);
        RemComp<GhostTakeoverAvailableComponent>(uid);
        _tag.AddTag(uid, "DoorBumpOpener");
    }

    /// <summary>
    ///     When failing to clone, much of the failed body is dissolved into a slurry of Ammonia and Blood, which spills from the machine.
    /// </summary>
    /// <remarks>
    ///     WOE BEFALLS WHOEVER FAILS TO CLONE A LAMIA
    /// </remarks>
    private void MakeAHugeMess(EntityUid uid, PhysicsComponent? physics = null, BloodstreamComponent? blood = null)
    {
        var tileMix = _atmosphereSystem.GetTileMixture(Transform(uid).GridUid, null, _transformSystem.GetGridTilePositionOrDefault((uid, Transform(uid))), true);
        Solution bloodSolution = new();

        tileMix?.AdjustMoles(Gas.Ammonia, 0.5f
            * ((physics is not null)
                ? physics.Mass
                : 71));

        bloodSolution.AddReagent("Blood", 0.8f
            * ((blood is not null)
                ? blood.BloodMaxVolume
                : 300));

        _puddleSystem.TrySpillAt(uid, bloodSolution, out _);
    }

    /// <summary>
    ///     Modify the clone's hunger and thirst values by an amount set in the cloningPod.
    /// </summary>
    private void UpdateHungerAndThirst(EntityUid uid, CloningPodComponent cloningPod)
    {
        if (cloningPod.HungerAdjustment != 0
            && TryComp<HungerComponent>(uid, out var hungerComponent))
            _hunger.SetHunger(uid, cloningPod.HungerAdjustment, hungerComponent);

        if (cloningPod.ThirstAdjustment != 0
            && TryComp<ThirstComponent>(uid, out var thirstComponent))
            _thirst.SetThirst(uid, thirstComponent, cloningPod.ThirstAdjustment);

        if (cloningPod.DrunkTimer != 0)
            _drunk.TryApplyDrunkenness(uid, cloningPod.DrunkTimer);
    }

    /// <summary>
    ///     Updates the HumanoidAppearanceComponent of the clone.
    ///     If a species swap is occuring, this updates all relevant information as per server config.
    /// </summary>
    private void UpdateCloneAppearance(
        EntityUid mob,
        HumanoidCharacterProfile pref,
        HumanoidAppearanceComponent humanoid,
        List<Sex> sexes,
        Gender oldGender,
        bool switchingSpecies,
        bool forceOldProfile,
        out Gender gender)
    {
        gender = oldGender;
        if (!TryComp<HumanoidAppearanceComponent>(mob, out var newHumanoid))
            return;

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
            else gender = humanoid.Gender;

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

            _humanoidSystem.LoadProfile(mob, pref);
            return;
        }
        _humanoidSystem.LoadProfile(mob, pref);
    }

    /// <summary>
    ///     Optionally makes sure that pronoun preferences are preserved by the clone.
    ///     Although handled here, the swap (if it occurs) happens during UpdateCloneAppearance.
    /// </summary>
    /// <param name="mob"></param>
    /// <param name="gender"></param>
    private void UpdateGrammar(EntityUid mob, Gender gender)
    {
        var grammar = EnsureComp<GrammarComponent>(mob);
        grammar.ProperNoun = true;
        grammar.Gender = gender;
        Dirty(mob, grammar);
    }

    /// <summary>
    ///     Optionally puts the clone in crit with high Cellular damage.
    ///     Medbay should use Cryogenics to "Finish" clones. Doxarubixadone is perfect for this.
    /// </summary>
    private void UpdateCloneDamage(EntityUid mob, CloningPodComponent clonePodComp, float geneticDamage)
    {
        if (!clonePodComp.DoGeneticDamage
            || !HasComp<DamageableComponent>(mob)
            || !_thresholds.TryGetThresholdForState(mob, Shared.Mobs.MobState.Critical, out var threshold))
            return;
        DamageSpecifier damage = new();
        damage.DamageDict.Add("Cellular", (int) threshold + 1 + geneticDamage);
        _damageable.TryChangeDamage(mob, damage, true);
    }
}
