using Robust.Shared.Timing;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Antag;
using Content.Shared.Changeling;
using Robust.Shared.Player;
using Content.Shared.Mind.Components;
using Robust.Shared.Random;
using Content.Shared.Bed.Sleep;
using Content.Shared.Popups;
using Content.Shared.Jittering;
using Content.Shared.Stunnable;
using Content.Server.Medical;
using Content.Shared.Tag;
using Content.Shared.Implants;

namespace Content.Server.Changeling;

public sealed partial class ChangelingInfectionSystem : EntitySystem
{
    private const string ChangelingRule = "Changeling";
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedJitteringSystem _jitterSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly VomitSystem _vomit = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingInfectionImplantComponent, ImplantImplantedEvent>(OnImplanterInjected);
    }

    private void OnImplanterInjected(EntityUid uid, ChangelingInfectionImplantComponent comp, ImplantImplantedEvent ev)
    {
        if (!_tag.HasTag(ev.Implant, comp.ChangelingInfectionImplant) || ev.Implanted == null)
            return;

        if (!EntityManager.TryGetComponent(ev.Implanted.Value, out AbsorbableComponent? _))
        {
            _popupSystem.PopupEntity(Loc.GetString(comp.ImplantFailPopup), ev.Implanted.Value, ev.Implanted.Value, comp.ImplantFailPopupType);
            return;
        }

        EnsureComp<ChangelingInfectionComponent>(ev.Implanted.Value);

        _popupSystem.PopupEntity(Loc.GetString(comp.ImplantPopup), ev.Implanted.Value, ev.Implanted.Value, comp.ImplantPopupType);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        foreach (var comp in EntityManager.EntityQuery<ChangelingInfectionComponent>())
        {
            var uid = comp.Owner;

            if (!EntityManager.TryGetComponent(uid, out AbsorbableComponent? _))
            {
                EntityManager.RemoveComponent<ChangelingInfectionComponent>(uid);
                return;
            }

            if (!comp.NeedsInitialization)
            {
                comp.FirstSymptoms = _timing.CurTime + comp.FirstSymptomsDelay;

                comp.KnockedOut = _timing.CurTime + comp.KnockedOutDelay;

                comp.FullyInfected = _timing.CurTime + comp.FullyInfectedDelay;
            }

            if (_timing.CurTime > comp.FirstSymptoms)
            {
                comp.CurrentState = ChangelingInfectionComponent.InfectionState.FirstSymptoms;
                comp.FirstSymptoms = _timing.CurTime + comp.SymptomProgressionTime; // Don't fire again
            }
            else if (_timing.CurTime > comp.KnockedOut)
            {
                comp.CurrentState = ChangelingInfectionComponent.InfectionState.KnockedOut;
                comp.KnockedOut = _timing.CurTime + comp.SymptomProgressionTime; // Hacky solution 2: Electric Boogaloo
            }
            else if (_timing.CurTime > comp.FullyInfected)
            {
                comp.CurrentState = ChangelingInfectionComponent.InfectionState.FullyInfected;
                comp.FullyInfected = _timing.CurTime + comp.SymptomProgressionTime; // Ehhhhh nobody's gonna see this the component is getting removed in a tick anyway!
            }

            if (_timing.CurTime < comp.EffectsTimer)
                continue;

            comp.EffectsTimer = _timing.CurTime + TimeSpan.FromSeconds(comp.EffectsTimerDelay);

            if (comp.NeedsInitialization)
                DoEffects(uid, comp);

            comp.NeedsInitialization = true; // First tick over, setup's complete, we can do the stuff now

        }
    }
    public void DoEffects(EntityUid uid, ChangelingInfectionComponent comp)
    {
        // Switch statement to determine which stage of infection we're in

        switch (comp.CurrentState)
        {
            case ChangelingInfectionComponent.InfectionState.FirstSymptoms:
                if (_random.Prob(comp.ScarySymptomChance))
                {
                    var funnyNumber = _random.Next(0, 4);
                    switch (funnyNumber)
                    {
                        case 1:
                            _popupSystem.PopupEntity(Loc.GetString(comp.ConvertWarningThrowupText), uid, uid, comp.ConvertThrowupPopupType);
                            _vomit.Vomit(uid);
                            break;
                        case 2:
                            _popupSystem.PopupEntity(Loc.GetString(comp.ConvertWarningCollapseText), uid, uid, comp.ConvertCollapsePopupType);
                            _stun.TryParalyze(uid, TimeSpan.FromSeconds(comp.ConvertParalyzeTime), true);
                            break;
                        case 3:
                            _popupSystem.PopupEntity(Loc.GetString(comp.ConvertWarningShakeText), uid, uid, comp.ConvertShakePopupType);
                            _jitterSystem.DoJitter(uid, TimeSpan.FromSeconds(comp.ConvertJitterTime),
                                false, comp.ConvertJitterAmplitude, comp.ConvertJitterFrequency);
                            break;
                    }
                    break;
                }

                _popupSystem.PopupEntity(Loc.GetString(_random.Pick(comp.SymptomMessages)), uid, uid);


                break;
            case ChangelingInfectionComponent.InfectionState.KnockedOut:
                // Add forced knocked out component
                if (!EntityManager.HasComponent<ForcedSleepingComponent>(uid))
                {
                    EntityManager.AddComponent<ForcedSleepingComponent>(uid);
                    _popupSystem.PopupEntity(Loc.GetString(comp.ConvertEepedText), uid, uid, comp.ConvertEeepedPopupType);
                    break;
                }
                if (_random.Prob(comp.ScarySymptomChance))
                {
                    _jitterSystem.DoJitter(uid, TimeSpan.FromSeconds(comp.ConvertJitterTime),
                        false, comp.ConvertJitterAmplitude, comp.ConvertJitterFrequency);
                    _popupSystem.PopupEntity(Loc.GetString(comp.ConvertEeepedShakeText), uid, uid, comp.ConvertEeepedShakePopupType);
                    break;
                }
                _popupSystem.PopupEntity(Loc.GetString(_random.Pick(comp.EepyMessages)), uid, uid);
                break;
            case ChangelingInfectionComponent.InfectionState.FullyInfected:
                // This will totally have no adverse effects whatsoever!
                if (!HasComp<MindContainerComponent>(uid) || !TryComp(uid, out ActorComponent? targetActor))
                    return;
                _antag.ForceMakeAntag<ChangelingRuleComponent>(targetActor.PlayerSession, ChangelingRule);

                EntityManager.RemoveComponent<ChangelingInfectionComponent>(uid);

                _popupSystem.PopupEntity(Loc.GetString(comp.ConvertSkillIssue), uid, uid);
                if (EntityManager.HasComponent<ForcedSleepingComponent>(uid))
                    EntityManager.RemoveComponent<ForcedSleepingComponent>(uid);

                break;
            case ChangelingInfectionComponent.InfectionState.None:
                break;
        }
    }
}

