using Content.Shared.CCVar;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Mobs.Systems;
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

        #region Mass Contests
        /// <summary>
        ///     Outputs the ratio of mass between a performer and the average human mass
        /// </summary>
        /// <param name="performerUid">Uid of Performer</param>
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
        /// <param name="performerPhysics"></param>
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
        /// <param name="performerUid"></param>
        /// <param name="targetUid"></param>
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
        ///     These cannot be implemented until AFTER the psychic refactor, but can still be factored into other systems before that point.
        ///     Same rule here as other Contest functions, simply multiply or divide by the function.
        /// </summary>
        /// <param name="performer"></param>
        /// <param name="bypassClamp"></param>
        /// <param name="rangeFactor"></param>
        /// <returns></returns>
        public float MindContest(EntityUid performer, bool bypassClamp = false, float rangeFactor = 1f)
        {
            if (!_cfg.GetCVar(CCVars.DoContestsSystem)
                || !_cfg.GetCVar(CCVars.DoMindContests))
                return 1f;

            return 1f;
        }

        public float MindContest(EntityUid performer, EntityUid target, bool bypassClamp = false, float rangeFactor = 1f)
        {
            if (!_cfg.GetCVar(CCVars.DoContestsSystem)
                || !_cfg.GetCVar(CCVars.DoMindContests))
                return 1f;

            return 1f;
        }

        #endregion

        #region EVERY CONTESTS

        public float EveryContest(
        	EntityUid performer,
            bool bypassClampMass = false,
            bool bypassClampStamina = false,
            bool bypassClampHealth = false,
            bool bypassClampMind = false,
            float rangeFactorMass = 1f,
            float rangeFactorStamina = 1f,
            float rangeFactorHealth = 1f,
            float rangeFactorMind = 1f,
            float weightMass = 1f,
            float weightStamina = 1f,
            float weightHealth = 1f,
            float weightMind = 1f,
            bool sumOrMultiply = false)
        {
            if (!_cfg.GetCVar(CCVars.DoContestsSystem))
                return 1f;

            var weightTotal = weightMass + weightStamina + weightHealth + weightMind;
            var massMultiplier = weightMass / weightTotal;
            var staminaMultiplier = weightStamina / weightTotal;
            var healthMultiplier = weightHealth / weightTotal;
            var mindMultiplier = weightMind / weightTotal;

            return sumOrMultiply
                ? MassContest(performer, bypassClampMass, rangeFactorMass) * massMultiplier
                    + StaminaContest(performer, bypassClampStamina, rangeFactorStamina) * staminaMultiplier
                    + HealthContest(performer, bypassClampHealth, rangeFactorHealth) * healthMultiplier
                    + MindContest(performer, bypassClampMind, rangeFactorMind) * mindMultiplier
                : MassContest(performer, bypassClampMass, rangeFactorMass) * massMultiplier
                    * StaminaContest(performer, bypassClampStamina, rangeFactorStamina) * staminaMultiplier
                    * HealthContest(performer, bypassClampHealth, rangeFactorHealth) * healthMultiplier
                    * MindContest(performer, bypassClampMind, rangeFactorMind) * mindMultiplier;
        }

        public float EveryContest(
        	EntityUid performer,
        	EntityUid target,
            bool bypassClampMass = false,
            bool bypassClampStamina = false,
            bool bypassClampHealth = false,
            bool bypassClampMind = false,
            float rangeFactorMass = 1f,
            float rangeFactorStamina = 1f,
            float rangeFactorHealth = 1f,
            float rangeFactorMind = 1f,
            float weightMass = 1f,
            float weightStamina = 1f,
            float weightHealth = 1f,
            float weightMind = 1f,
            bool sumOrMultiply = false)
        {
            if (!_cfg.GetCVar(CCVars.DoContestsSystem))
                return 1f;

            var weightTotal = weightMass + weightStamina + weightHealth + weightMind;
            var massMultiplier = weightMass / weightTotal;
            var staminaMultiplier = weightStamina / weightTotal;
            var healthMultiplier = weightHealth / weightTotal;
            var mindMultiplier = weightMind / weightTotal;

            return sumOrMultiply
                ? MassContest(performer, target, bypassClampMass, rangeFactorMass) * massMultiplier
                    + StaminaContest(performer, target, bypassClampStamina, rangeFactorStamina) * staminaMultiplier
                    + HealthContest(performer, target, bypassClampHealth, rangeFactorHealth) * healthMultiplier
                    + MindContest(performer, target, bypassClampMind, rangeFactorMind) * mindMultiplier
                : MassContest(performer, target, bypassClampMass, rangeFactorMass) * massMultiplier
                    * StaminaContest(performer, target, bypassClampStamina, rangeFactorStamina) * staminaMultiplier
                    * HealthContest(performer, target, bypassClampHealth, rangeFactorHealth) * healthMultiplier
                    * MindContest(performer, target, bypassClampMind, rangeFactorMind) * mindMultiplier;
        }
        #endregion
    }
}
