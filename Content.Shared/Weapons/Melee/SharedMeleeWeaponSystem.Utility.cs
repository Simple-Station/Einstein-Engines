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
                * (component.DoMassInteraction ? ((!component.MassDisadvantage
                    ? _contests.MassContest(user, component.MassBypassClamp, component.MassRangeModifier)
                    : 1 / _contests.MassContest(user, component.MassBypassClamp, component.MassRangeModifier))
                        + component.MassOffset)
                            : 1)
                * (component.DoStaminaInteraction ? ((!component.StaminaDisadvantage
                    ? _contests.StaminaContest(user, component.StaminaBypassClamp, component.StaminaRangeModifier)
                    : 1 / _contests.StaminaContest(user, component.StaminaBypassClamp, component.StaminaRangeModifier))
                        + component.StaminaOffset)
                            : 1)
                * (component.DoHealthInteraction ? ((!component.HealthDisadvantage
                    ? _contests.HealthContest(user, component.HealthBypassClamp, component.HealthRangeModifier)
                    : 1 / _contests.HealthContest(user, component.HealthBypassClamp, component.HealthRangeModifier))
                        + component.HealthOffset)
                            : 1)
                * (component.DoMindInteraction ? ((!component.MindDisadvantage
                    ? _contests.MindContest(user, component.MindBypassClamp, component.MindRangeModifier)
                    : 1 / _contests.MindContest(user, component.MindBypassClamp, component.MindRangeModifier))
                        + component.MindOffset)
                            : 1)
                * (component.DoMoodInteraction ? ((!component.MoodDisadvantage
                    ? _contests.MoodContest(user, component.MoodBypassClamp, component.MoodRangeModifier)
                    : 1 / _contests.MoodContest(user, component.MoodBypassClamp, component.MoodRangeModifier))
                        + component.MoodOffset)
                            : 1)
                * (component.DoEveryInteraction ? (!component.EveryDisadvantage
                    ? _contests.EveryContest(user,
                        component.MassBypassClamp,
                        component.StaminaBypassClamp,
                        component.HealthBypassClamp,
                        component.MindBypassClamp,
                        component.MoodBypassClamp,
                        component.MassRangeModifier,
                        component.StaminaRangeModifier,
                        component.HealthRangeModifier,
                        component.MindRangeModifier,
                        component.MoodRangeModifier,
                        component.EveryMassWeight,
                        component.EveryStaminaWeight,
                        component.EveryHealthWeight,
                        component.EveryMindWeight,
                        component.EveryMoodWeight,
                        component.EveryInteractionSumOrMultiply)
                    : 1 / _contests.EveryContest(user,
                            component.MassBypassClamp,
                            component.StaminaBypassClamp,
                            component.HealthBypassClamp,
                            component.MindBypassClamp,
                            component.MoodBypassClamp,
                            component.MassRangeModifier,
                            component.StaminaRangeModifier,
                            component.HealthRangeModifier,
                            component.MindRangeModifier,
                            component.MoodRangeModifier,
                            component.EveryMassWeight,
                            component.EveryStaminaWeight,
                            component.EveryHealthWeight,
                            component.EveryMindWeight,
                            component.EveryMoodWeight,
                            component.EveryInteractionSumOrMultiply))
                                : 1);
    }
}