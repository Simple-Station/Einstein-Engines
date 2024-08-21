using Content.Shared.Abilities.Psionics;
using Content.Shared.CCVar;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Mood;
using Robust.Shared.Configuration;
using Robust.Shared.Physics.Components;

namespace Content.Shared.Contests
{
    public sealed partial class ContestsSystem : EntitySystem
    {
        [Dependency] private readonly IConfigurationManager _cfg = default!;
        [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;

        /// <summary>
        ///     The presumed average mass of a player entity
        ///     Defaulted to the average mass of an adult human
        /// </summary>
        private const float AverageMass = 71f;

        /// <summary>
        ///     The presumed average sum of a Psionic's Baseline Amplification and Baseline Dampening.
        ///     Since Baseline casting stats are a random value between 0.4 and 1.2, this is defaulted to 0.8 + 0.8.
        /// </summary>
        private const float AveragePsionicPotential = 1.6f;

        #region Mass Contests
        /// <summary>
        ///     Outputs the ratio of mass between a performer and the average human mass
        /// </summary>
        public float MassContest(EntityUid performerUid, bool bypassClamp = false, float rangeFactor = 1f, float otherMass = AverageMass)
        {
            if (!_cfg.GetCVar(CCVars.DoContestsSystem)
                || !_cfg.GetCVar(CCVars.DoMassContests)
                || !TryComp<PhysicsComponent>(performerUid, out var performerPhysics)
                || performerPhysics.Mass == 0)
                return 1f;

            return _cfg.GetCVar(CCVars.AllowClampOverride) && bypassClamp
                ? performerPhysics.Mass / otherMass
                : Math.Clamp(performerPhysics.Mass / otherMass,
                    1 - _cfg.GetCVar(CCVars.MassContestsMaxPercentage) * rangeFactor,
                    1 + _cfg.GetCVar(CCVars.MassContestsMaxPercentage) * rangeFactor);
        }

        /// <inheritdoc cref="MassContest(EntityUid, bool, float, float)"/>
        /// <remarks>
        ///     MaybeMassContest, in case your entity doesn't exist
        /// </remarks>
        public float MassContest(EntityUid? performerUid, bool bypassClamp = false, float rangeFactor = 1f, float otherMass = AverageMass)
        {
            if (!_cfg.GetCVar(CCVars.DoContestsSystem)
                || !_cfg.GetCVar(CCVars.DoMassContests)
                || performerUid is null)
                return 1f;

            return MassContest(performerUid.Value, bypassClamp, rangeFactor, otherMass);
        }

        /// <summary>
        ///     Outputs the ratio of mass between a performer and the average human mass
        ///     If a function already has the performer's physics component, this is faster
        /// </summary>
        public float MassContest(PhysicsComponent performerPhysics, bool bypassClamp = false, float rangeFactor = 1f, float otherMass = AverageMass)
        {
            if (!_cfg.GetCVar(CCVars.DoContestsSystem)
                || !_cfg.GetCVar(CCVars.DoMassContests)
                || performerPhysics.Mass == 0)
                return 1f;

            return _cfg.GetCVar(CCVars.AllowClampOverride) && bypassClamp
                ? performerPhysics.Mass / otherMass
                : Math.Clamp(performerPhysics.Mass / otherMass,
                    1 - _cfg.GetCVar(CCVars.MassContestsMaxPercentage) * rangeFactor,
                    1 + _cfg.GetCVar(CCVars.MassContestsMaxPercentage) * rangeFactor);
        }

        /// <summary>
        ///     Outputs the ratio of mass between a performer and a target, accepts either EntityUids or PhysicsComponents in any combination
        ///     If you have physics components already in your function, use <see cref="MassContest(PhysicsComponent, float)" /> instead
        /// </summary>
        public float MassContest(EntityUid performerUid, EntityUid targetUid, bool bypassClamp = false, float rangeFactor = 1f)
        {
            if (!_cfg.GetCVar(CCVars.DoContestsSystem)
                || !_cfg.GetCVar(CCVars.DoMassContests)
                || !TryComp<PhysicsComponent>(performerUid, out var performerPhysics)
                || !TryComp<PhysicsComponent>(targetUid, out var targetPhysics)
                || performerPhysics.Mass == 0
                || targetPhysics.InvMass == 0)
                return 1f;

            return _cfg.GetCVar(CCVars.AllowClampOverride) && bypassClamp
                ? performerPhysics.Mass * targetPhysics.InvMass
                : Math.Clamp(performerPhysics.Mass * targetPhysics.InvMass,
                    1 - _cfg.GetCVar(CCVars.MassContestsMaxPercentage) * rangeFactor,
                    1 + _cfg.GetCVar(CCVars.MassContestsMaxPercentage) * rangeFactor);
        }

        /// <inheritdoc cref="MassContest(EntityUid, EntityUid, bool, float)"/>
        public float MassContest(EntityUid performerUid, PhysicsComponent targetPhysics, bool bypassClamp = false, float rangeFactor = 1f)
        {
            if (!_cfg.GetCVar(CCVars.DoContestsSystem)
                || !_cfg.GetCVar(CCVars.DoMassContests)
                || !TryComp<PhysicsComponent>(performerUid, out var performerPhysics)
                || performerPhysics.Mass == 0
                || targetPhysics.InvMass == 0)
                return 1f;

            return _cfg.GetCVar(CCVars.AllowClampOverride) && bypassClamp
                ? performerPhysics.Mass * targetPhysics.InvMass
                : Math.Clamp(performerPhysics.Mass * targetPhysics.InvMass,
                    1 - _cfg.GetCVar(CCVars.MassContestsMaxPercentage) * rangeFactor,
                    1 + _cfg.GetCVar(CCVars.MassContestsMaxPercentage) * rangeFactor);
        }

        /// <inheritdoc cref="MassContest(EntityUid, EntityUid, bool, float)"/>
        public float MassContest(PhysicsComponent performerPhysics, EntityUid targetUid, bool bypassClamp = false, float rangeFactor = 1f)
        {
            if (!_cfg.GetCVar(CCVars.DoContestsSystem)
                || !_cfg.GetCVar(CCVars.DoMassContests)
                || !TryComp<PhysicsComponent>(targetUid, out var targetPhysics)
                || performerPhysics.Mass == 0
                || targetPhysics.InvMass == 0)
                return 1f;

            return _cfg.GetCVar(CCVars.AllowClampOverride) && bypassClamp
                ? performerPhysics.Mass * targetPhysics.InvMass
                : Math.Clamp(performerPhysics.Mass * targetPhysics.InvMass,
                    1 - _cfg.GetCVar(CCVars.MassContestsMaxPercentage) * rangeFactor,
                    1 + _cfg.GetCVar(CCVars.MassContestsMaxPercentage) * rangeFactor);
        }

        /// <inheritdoc cref="MassContest(EntityUid, EntityUid, bool, float)"/>
        public float MassContest(PhysicsComponent performerPhysics, PhysicsComponent targetPhysics, bool bypassClamp = false, float rangeFactor = 1f)
        {
            if (!_cfg.GetCVar(CCVars.DoContestsSystem)
                || !_cfg.GetCVar(CCVars.DoMassContests)
                || performerPhysics.Mass == 0
                || targetPhysics.InvMass == 0)
                return 1f;

            return _cfg.GetCVar(CCVars.AllowClampOverride) && bypassClamp
                ? performerPhysics.Mass * targetPhysics.InvMass
                : Math.Clamp(performerPhysics.Mass * targetPhysics.InvMass,
                    1 - _cfg.GetCVar(CCVars.MassContestsMaxPercentage) * rangeFactor,
                    1 + _cfg.GetCVar(CCVars.MassContestsMaxPercentage) * rangeFactor);
        }

        #endregion
        #region Stamina Contests

        public float StaminaContest(EntityUid performer, bool bypassClamp = false, float rangeFactor = 1f)
        {
            if (!_cfg.GetCVar(CCVars.DoContestsSystem)
                || !_cfg.GetCVar(CCVars.DoStaminaContests)
                || !TryComp<StaminaComponent>(performer, out var perfStamina)
                || perfStamina.StaminaDamage == 0)
                return 1f;

            return _cfg.GetCVar(CCVars.AllowClampOverride) && bypassClamp
                ? 1 - perfStamina.StaminaDamage / perfStamina.CritThreshold
                : 1 - Math.Clamp(perfStamina.StaminaDamage / perfStamina.CritThreshold, 0, 0.25f * rangeFactor);
        }

        public float StaminaContest(StaminaComponent perfStamina, bool bypassClamp = false, float rangeFactor = 1f)
        {
            if (!_cfg.GetCVar(CCVars.DoContestsSystem)
                || !_cfg.GetCVar(CCVars.DoStaminaContests))
                return 1f;

            return _cfg.GetCVar(CCVars.AllowClampOverride) && bypassClamp
                ? 1 - perfStamina.StaminaDamage / perfStamina.CritThreshold
                : 1 - Math.Clamp(perfStamina.StaminaDamage / perfStamina.CritThreshold, 0, 0.25f * rangeFactor);
        }

        public float StaminaContest(EntityUid performer, EntityUid target, bool bypassClamp = false, float rangeFactor = 1f)
        {
            if (!_cfg.GetCVar(CCVars.DoContestsSystem)
                || !_cfg.GetCVar(CCVars.DoStaminaContests)
                || !TryComp<StaminaComponent>(performer, out var perfStamina)
                || !TryComp<StaminaComponent>(target, out var targetStamina))
                return 1f;

            return _cfg.GetCVar(CCVars.AllowClampOverride) && bypassClamp
                ? (1 - perfStamina.StaminaDamage / perfStamina.CritThreshold)
                    / (1 - targetStamina.StaminaDamage / targetStamina.CritThreshold)
                : (1 - Math.Clamp(perfStamina.StaminaDamage / perfStamina.CritThreshold, 0, 0.25f * rangeFactor))
                    / (1 - Math.Clamp(targetStamina.StaminaDamage / targetStamina.CritThreshold, 0, 0.25f * rangeFactor));
        }

        #endregion

        #region Health Contests

        public float HealthContest(EntityUid performer, bool bypassClamp = false, float rangeFactor = 1f)
        {
            if (!_cfg.GetCVar(CCVars.DoContestsSystem)
                || !_cfg.GetCVar(CCVars.DoHealthContests)
                || !TryComp<DamageableComponent>(performer, out var damage)
                || !_mobThreshold.TryGetThresholdForState(performer, Mobs.MobState.Critical, out var threshold))
                return 1f;

            return _cfg.GetCVar(CCVars.AllowClampOverride) && bypassClamp
                ? 1 - damage.TotalDamage.Float() / threshold.Value.Float()
                : 1 - Math.Clamp(damage.TotalDamage.Float() / threshold.Value.Float(), 0, 0.25f * rangeFactor);
        }

        public float HealthContest(EntityUid performer, EntityUid target, bool bypassClamp = false, float rangeFactor = 1f)
        {
            if (!_cfg.GetCVar(CCVars.DoContestsSystem)
                || !_cfg.GetCVar(CCVars.DoHealthContests)
                || !TryComp<DamageableComponent>(performer, out var perfDamage)
                || !TryComp<DamageableComponent>(target, out var targetDamage)
                || !_mobThreshold.TryGetThresholdForState(performer, Mobs.MobState.Critical, out var perfThreshold)
                || !_mobThreshold.TryGetThresholdForState(target, Mobs.MobState.Critical, out var targetThreshold))
                return 1f;

            return _cfg.GetCVar(CCVars.AllowClampOverride) && bypassClamp
                ? (1 - perfDamage.TotalDamage.Float() / perfThreshold.Value.Float())
                    / (1 - targetDamage.TotalDamage.Float() / targetThreshold.Value.Float())
                : (1 - Math.Clamp(perfDamage.TotalDamage.Float() / perfThreshold.Value.Float(), 0, 0.25f * rangeFactor))
                    / (1 - Math.Clamp(targetDamage.TotalDamage.Float() / targetThreshold.Value.Float(), 0, 0.25f * rangeFactor));
        }
        #endregion

        #region Mind Contests

        /// <summary>
        ///     Returns the ratio of casting stats between a performer and the presumed average latent psionic.
        ///     Uniquely among Contests, not being Psionic is not a failure condition, and is instead a variable.
        ///     If you do not have a PsionicComponent, your combined casting stats are assumed to be 0.1f
        /// </summary>
        /// <remarks>
        ///     This can produce some truly astounding modifiers, so be ready to meet god if you bypass the clamp.
        /// </remarks>
        public float MindContest(EntityUid performer, bool bypassClamp = false, float rangeFactor = 1f, float otherPsion = AveragePsionicPotential)
        {
            if (!_cfg.GetCVar(CCVars.DoContestsSystem)
                || !_cfg.GetCVar(CCVars.DoMindContests))
                return 1f;

            var performerPotential = TryComp<PsionicComponent>(performer, out var performerPsionic)
                ? performerPsionic.CurrentAmplification + performerPsionic.CurrentDampening
                : 0.1f;

            if (performerPotential == otherPsion)
                return 1f;

            return _cfg.GetCVar(CCVars.AllowClampOverride) && bypassClamp
                ? performerPotential / otherPsion
                : Math.Clamp(performerPotential / otherPsion,
                    1 - _cfg.GetCVar(CCVars.MassContestsMaxPercentage) * rangeFactor,
                    1 + _cfg.GetCVar(CCVars.MassContestsMaxPercentage) * rangeFactor);
        }

        /// <summary>
        ///     Returns the ratio of casting stats between a performer and a target.
        ///     Like with single-Uid MindContests, if an entity does not possess a PsionicComponent, its casting stats are assumed to be 0.1f.
        /// </summary>
        /// <remarks>
        ///     This can produce some truly astounding modifiers, so be ready to meet god if you bypass the clamp.
        /// </remarks>
        public float MindContest(EntityUid performer, EntityUid target, bool bypassClamp = false, float rangeFactor = 1f)
        {
            if (!_cfg.GetCVar(CCVars.DoContestsSystem)
                || !_cfg.GetCVar(CCVars.DoMindContests))
                return 1f;

            var performerPotential = TryComp<PsionicComponent>(performer, out var performerPsionic)
                ? performerPsionic.CurrentAmplification + performerPsionic.CurrentDampening
                : 0.1f;

            var targetPotential = TryComp<PsionicComponent>(target, out var targetPsionic)
                ? targetPsionic.CurrentAmplification + targetPsionic.CurrentDampening
                : 0.1f;

            if (performerPotential == targetPotential)
                return 1f;

            return _cfg.GetCVar(CCVars.AllowClampOverride) && bypassClamp
                ? performerPotential / targetPotential
                : Math.Clamp(performerPotential / targetPotential,
                    1 - _cfg.GetCVar(CCVars.MassContestsMaxPercentage) * rangeFactor,
                    1 + _cfg.GetCVar(CCVars.MassContestsMaxPercentage) * rangeFactor);
        }

        #endregion

        #region Mood Contests

        /// <summary>
        ///     Outputs the ratio of an Entity's mood level and its Neutral Mood threshold.
        /// </summary>
        public float MoodContest(EntityUid performer, bool bypassClamp = false, float rangeFactor = 1f)
        {
            if (!_cfg.GetCVar(CCVars.DoContestsSystem)
                || !_cfg.GetCVar(CCVars.DoMoodContests)
                || !TryComp<NetMoodComponent>(performer, out var mood))
                return 1f;

            return _cfg.GetCVar(CCVars.AllowClampOverride) && bypassClamp
                ? mood.CurrentMoodLevel / mood.NeutralMoodThreshold
                : Math.Clamp(mood.CurrentMoodLevel / mood.NeutralMoodThreshold,
                    1 - _cfg.GetCVar(CCVars.MassContestsMaxPercentage) * rangeFactor,
                    1 + _cfg.GetCVar(CCVars.MassContestsMaxPercentage) * rangeFactor);
        }

        /// <summary>
        ///     Outputs the ratio of mood level between two Entities.
        /// </summary>
        public float MoodContest(EntityUid performer, EntityUid target, bool bypassClamp = false, float rangeFactor = 1f)
        {
            if (!_cfg.GetCVar(CCVars.DoContestsSystem)
                || !_cfg.GetCVar(CCVars.DoMoodContests)
                || !TryComp<NetMoodComponent>(performer, out var performerMood)
                || !TryComp<NetMoodComponent>(target, out var targetMood))
                return 1f;

            return _cfg.GetCVar(CCVars.AllowClampOverride) && bypassClamp
                ? performerMood.CurrentMoodLevel / targetMood.CurrentMoodLevel
                : Math.Clamp(performerMood.CurrentMoodLevel / targetMood.CurrentMoodLevel,
                    1 - _cfg.GetCVar(CCVars.MassContestsMaxPercentage) * rangeFactor,
                    1 + _cfg.GetCVar(CCVars.MassContestsMaxPercentage) * rangeFactor);
        }

        #endregion

        #region EVERY CONTESTS

        public float EveryContest(
        	EntityUid performer,
            bool bypassClampMass = false,
            bool bypassClampStamina = false,
            bool bypassClampHealth = false,
            bool bypassClampMind = false,
            bool bypassClampMood = false,
            float rangeFactorMass = 1f,
            float rangeFactorStamina = 1f,
            float rangeFactorHealth = 1f,
            float rangeFactorMind = 1f,
            float rangeFactorMood = 1f,
            float weightMass = 1f,
            float weightStamina = 1f,
            float weightHealth = 1f,
            float weightMind = 1f,
            float weightMood = 1f,
            bool sumOrMultiply = false)
        {
            if (!_cfg.GetCVar(CCVars.DoContestsSystem))
                return 1f;

            var weightTotal = weightMass + weightStamina + weightHealth + weightMind + weightMood;
            var massMultiplier = weightMass / weightTotal;
            var staminaMultiplier = weightStamina / weightTotal;
            var healthMultiplier = weightHealth / weightTotal;
            var mindMultiplier = weightMind / weightTotal;
            var moodMultiplier = weightMood / weightTotal;

            return sumOrMultiply
                ? MassContest(performer, bypassClampMass, rangeFactorMass) * massMultiplier
                    + StaminaContest(performer, bypassClampStamina, rangeFactorStamina) * staminaMultiplier
                    + HealthContest(performer, bypassClampHealth, rangeFactorHealth) * healthMultiplier
                    + MindContest(performer, bypassClampMind, rangeFactorMind) * mindMultiplier
                    + MoodContest(performer, bypassClampMood, rangeFactorMood) * moodMultiplier
                : MassContest(performer, bypassClampMass, rangeFactorMass) * massMultiplier
                    * StaminaContest(performer, bypassClampStamina, rangeFactorStamina) * staminaMultiplier
                    * HealthContest(performer, bypassClampHealth, rangeFactorHealth) * healthMultiplier
                    * MindContest(performer, bypassClampMind, rangeFactorMind) * mindMultiplier
                    * MoodContest(performer, bypassClampMood, rangeFactorMood) * moodMultiplier;
        }

        public float EveryContest(
        	EntityUid performer,
        	EntityUid target,
            bool bypassClampMass = false,
            bool bypassClampStamina = false,
            bool bypassClampHealth = false,
            bool bypassClampMind = false,
            bool bypassClampMood = false,
            float rangeFactorMass = 1f,
            float rangeFactorStamina = 1f,
            float rangeFactorHealth = 1f,
            float rangeFactorMind = 1f,
            float rangeFactorMood = 1f,
            float weightMass = 1f,
            float weightStamina = 1f,
            float weightHealth = 1f,
            float weightMind = 1f,
            float weightMood = 1f,
            bool sumOrMultiply = false)
        {
            if (!_cfg.GetCVar(CCVars.DoContestsSystem))
                return 1f;

            var weightTotal = weightMass + weightStamina + weightHealth + weightMind + weightMood;
            var massMultiplier = weightMass / weightTotal;
            var staminaMultiplier = weightStamina / weightTotal;
            var healthMultiplier = weightHealth / weightTotal;
            var mindMultiplier = weightMind / weightTotal;
            var moodMultiplier = weightMood / weightTotal;

            return sumOrMultiply
                ? MassContest(performer, target, bypassClampMass, rangeFactorMass) * massMultiplier
                    + StaminaContest(performer, target, bypassClampStamina, rangeFactorStamina) * staminaMultiplier
                    + HealthContest(performer, target, bypassClampHealth, rangeFactorHealth) * healthMultiplier
                    + MindContest(performer, target, bypassClampMind, rangeFactorMind) * mindMultiplier
                    + MoodContest(performer, target, bypassClampMood, rangeFactorMood) * moodMultiplier
                : MassContest(performer, target, bypassClampMass, rangeFactorMass) * massMultiplier
                    * StaminaContest(performer, target, bypassClampStamina, rangeFactorStamina) * staminaMultiplier
                    * HealthContest(performer, target, bypassClampHealth, rangeFactorHealth) * healthMultiplier
                    * MindContest(performer, target, bypassClampMind, rangeFactorMind) * mindMultiplier
                    * MoodContest(performer, target, bypassClampMood, rangeFactorMood) * moodMultiplier;
        }
        #endregion
    }
}
