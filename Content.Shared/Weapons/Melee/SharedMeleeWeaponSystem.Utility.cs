using Content.Shared.CCVar;

namespace Content.Shared.Weapons.Melee;

public abstract partial class SharedMeleeWeaponSystem : EntitySystem
{
    /// <summary>
    ///     Constructor for feeding options from a given MeleeWeaponComponent into the ContestsSystem.
    ///     Just multiply by this and give it a user EntityUid and a MeleeWeapon component. That's all you need to know.
    /// </summary>
    public float MeleeContestInteractions(EntityUid user, MeleeWeaponComponent component)
    {
        if (!_config.GetCVar(CCVars.DoContestsSystem))
            return 1;

        return 1
                * (component.ContestArgs.DoMassInteraction ? ((!component.ContestArgs.MassDisadvantage
                    ? _contests.MassContest(user, component.ContestArgs.MassBypassClamp, component.ContestArgs.MassRangeModifier)
                    : 1 / _contests.MassContest(user, component.ContestArgs.MassBypassClamp, component.ContestArgs.MassRangeModifier))
                        + component.ContestArgs.MassOffset)
                            : 1)
                * (component.ContestArgs.DoStaminaInteraction ? ((!component.ContestArgs.StaminaDisadvantage
                    ? _contests.StaminaContest(user, component.ContestArgs.StaminaBypassClamp, component.ContestArgs.StaminaRangeModifier)
                    : 1 / _contests.StaminaContest(user, component.ContestArgs.StaminaBypassClamp, component.ContestArgs.StaminaRangeModifier))
                        + component.ContestArgs.StaminaOffset)
                            : 1)
                * (component.ContestArgs.DoHealthInteraction ? ((!component.ContestArgs.HealthDisadvantage
                    ? _contests.HealthContest(user, component.ContestArgs.HealthBypassClamp, component.ContestArgs.HealthRangeModifier)
                    : 1 / _contests.HealthContest(user, component.ContestArgs.HealthBypassClamp, component.ContestArgs.HealthRangeModifier))
                        + component.ContestArgs.HealthOffset)
                            : 1)
                * (component.ContestArgs.DoMindInteraction ? ((!component.ContestArgs.MindDisadvantage
                    ? _contests.MindContest(user, component.ContestArgs.MindBypassClamp, component.ContestArgs.MindRangeModifier)
                    : 1 / _contests.MindContest(user, component.ContestArgs.MindBypassClamp, component.ContestArgs.MindRangeModifier))
                        + component.ContestArgs.MindOffset)
                            : 1)
                * (component.ContestArgs.DoMoodInteraction ? ((!component.ContestArgs.MoodDisadvantage
                    ? _contests.MoodContest(user, component.ContestArgs.MoodBypassClamp, component.ContestArgs.MoodRangeModifier)
                    : 1 / _contests.MoodContest(user, component.ContestArgs.MoodBypassClamp, component.ContestArgs.MoodRangeModifier))
                        + component.ContestArgs.MoodOffset)
                            : 1)
                * (component.ContestArgs.DoEveryInteraction ? (!component.ContestArgs.EveryDisadvantage
                    ? _contests.EveryContest(user,
                        component.ContestArgs.MassBypassClamp,
                        component.ContestArgs.StaminaBypassClamp,
                        component.ContestArgs.HealthBypassClamp,
                        component.ContestArgs.MindBypassClamp,
                        component.ContestArgs.MoodBypassClamp,
                        component.ContestArgs.MassRangeModifier,
                        component.ContestArgs.StaminaRangeModifier,
                        component.ContestArgs.HealthRangeModifier,
                        component.ContestArgs.MindRangeModifier,
                        component.ContestArgs.MoodRangeModifier,
                        component.ContestArgs.EveryMassWeight,
                        component.ContestArgs.EveryStaminaWeight,
                        component.ContestArgs.EveryHealthWeight,
                        component.ContestArgs.EveryMindWeight,
                        component.ContestArgs.EveryMoodWeight,
                        component.ContestArgs.EveryInteractionSumOrMultiply)
                    : 1 / _contests.EveryContest(user,
                            component.ContestArgs.MassBypassClamp,
                            component.ContestArgs.StaminaBypassClamp,
                            component.ContestArgs.HealthBypassClamp,
                            component.ContestArgs.MindBypassClamp,
                            component.ContestArgs.MoodBypassClamp,
                            component.ContestArgs.MassRangeModifier,
                            component.ContestArgs.StaminaRangeModifier,
                            component.ContestArgs.HealthRangeModifier,
                            component.ContestArgs.MindRangeModifier,
                            component.ContestArgs.MoodRangeModifier,
                            component.ContestArgs.EveryMassWeight,
                            component.ContestArgs.EveryStaminaWeight,
                            component.ContestArgs.EveryHealthWeight,
                            component.ContestArgs.EveryMindWeight,
                            component.ContestArgs.EveryMoodWeight,
                            component.ContestArgs.EveryInteractionSumOrMultiply))
                                : 1);
    }
}