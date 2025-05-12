using Content.Server._EE.Shadowling.Objectives;
using Content.Server.Actions;
using Content.Server.Humanoid;
using Content.Server.Language;
using Content.Server.Mind;
using Content.Server.Objectives;
using Content.Server.Popups;
using Content.Server.Roles;
using Content.Server.Storage.EntitySystems;
using Content.Shared._EE.Shadowling.Systems;
using Content.Shared._EE.Shadowling;
using Content.Shared._EE.Shadowling.Components;
using Content.Shared._Goobstation.Flashbang;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Inventory;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Objectives.Systems;
using Content.Shared.Popups;
using Content.Shared.Roles;
using Robust.Server.GameObjects;


namespace Content.Server._EE.Shadowling;


/// <summary>
/// This handles the Shadowling's System
/// </summary>
public sealed partial class ShadowlingSystem : SharedShadowlingSystem
{
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly EntityStorageSystem _entityStorage = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly AlertsSystem _alert = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly LanguageSystem _language = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly SharedObjectivesSystem _sharedObjectives = default!;
    [Dependency] private  readonly HumanoidAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingComponent, ComponentInit>(OnInit);

        SubscribeLocalEvent<ShadowlingComponent, BeforeDamageChangedEvent>(BeforeDamageChanged);

        SubscribeLocalEvent<ShadowlingComponent, MobStateChangedEvent>(OnMobStateChanged);

        SubscribeLocalEvent<ShadowlingComponent, GetFlashbangedEvent>(OnFlashBanged);
        SubscribeLocalEvent<ShadowlingComponent, DamageModifyEvent>(OnDamageModify);

        SubscribeAbilities();
    }

    #region Event Handlers

    private void OnMobStateChanged(EntityUid uid, ShadowlingComponent component, MobStateChangedEvent args)
    {
        // Remove all Thralls if shadowling is dead
        if (args.NewMobState == MobState.Dead || args.NewMobState == MobState.Invalid)
        {
            foreach (var thrall in component.Thralls)
            {
                _popup.PopupEntity(Loc.GetString("shadowling-dead"), thrall, thrall, PopupType.LargeCaution);
                RemCompDeferred<ThrallComponent>(thrall);
            }

            var ev = new ShadowlingDeathEvent();
            RaiseLocalEvent(ev);
        }
    }

    private void OnDamageModify(EntityUid uid, ShadowlingComponent component, DamageModifyEvent args)
    {
        foreach (var (key,_) in args.Damage.DamageDict)
        {
            if (key == "Heat")
                args.Damage += component.HeatDamageProjectileModifier;
        }
    }
    private void OnFlashBanged(EntityUid uid, ShadowlingComponent component, GetFlashbangedEvent args)
    {
        // Shadowling get damaged from flashbangs
        if (!TryComp<DamageableComponent>(uid, out var damageableComp))
            return;

        _damageable.TryChangeDamage(uid, component.HeatDamage, damageable: damageableComp);
    }
    public void OnThrallAdded(EntityUid uid, EntityUid thrall, ShadowlingComponent comp)
    {
        if (!TryComp<LightDetectionDamageModifierComponent>(uid, out var lightDet))
            return;

        if (lightDet == null)
            return;

        lightDet.ResistanceModifier += comp.LightResistanceModifier;
    }

    public void OnThrallRemoved(EntityUid uid, EntityUid thrall, ShadowlingComponent comp)
    {
        if (!TryComp<LightDetectionDamageModifierComponent>(uid, out var lightDet))
            return;

        if (lightDet == null)
            return;

        lightDet.ResistanceModifier -= comp.LightResistanceModifier;
    }

    private void OnInit(EntityUid uid, ShadowlingComponent component, ref ComponentInit args)
    {
        _language.AddLanguage(uid, component.SlingLanguageId);
        if (!TryComp(uid, out ActionsComponent? actions))
            return;
        _actions.AddAction(uid, ref component.ActionHatchEntity, component.ActionHatch, component: actions);
    }

    private void BeforeDamageChanged(EntityUid uid, ShadowlingComponent comp, BeforeDamageChangedEvent args)
    {
        // Can't take damage during hatching
        if (comp.IsHatching)
            args.Cancelled = true;
    }

    public void OnPhaseChanged(EntityUid uid, ShadowlingComponent component, ShadowlingPhases phase)
    {
        if (!TryComp<ActionsComponent>(uid, out var actions))
            return;

        if (phase == ShadowlingPhases.PostHatch)
        {
            // When the entity gets polymorphed, the OnInit starts so... We have to remove it again here.
            _actions.RemoveAction(uid, component.ActionHatchEntity);

            AddPostHatchActions(uid, component);

            // todo: uncomment after debugging abilities
            /*EnsureComp<LightDetectionComponent>(uid);
            var lightMod = EnsureComp<LightDetectionDamageModifierComponent>(uid);
            lightMod.ResistanceModifier = 0.5f; // Let them start with 50% resistance, and decrease it per Thrall
            _alert.ShowAlert(uid, component.AlertProto);*/
        }
        else if (phase == ShadowlingPhases.Ascension)
        {
            // Remove all previous actions
            foreach (var action in actions.Actions)
            {
                if (!HasComp<ShadowlingActionComponent>(action))
                    continue;

                _actions.RemoveAction(uid, action);
            }

            // give thralls partial ascension
            // destroy all lights

            var ev = new ShadowlingAscendEvent();
            RaiseLocalEvent(ev);



            AddComp<ShadowlingAnnihilateComponent>(uid);
            AddComp<ShadowlingHypnosisComponent>(uid);
            AddComp<ShadowlingPlaneShiftComponent>(uid);
            AddComp<ShadowlingLightningStormComponent>(uid);
            AddComp<ShadowlingAscendantBroadcastComponent>(uid);
            _actions.AddAction(uid, ref component.ActionAnnihilateEntity, component.ActionAnnihilate, component: actions);
            _actions.AddAction(uid, ref component.ActionHypnosisEntity, component.ActionHypnosis, component: actions);
            _actions.AddAction(uid, ref component.ActionPlaneShiftEntity, component.ActionPlaneShift, component: actions);
            _actions.AddAction(uid, ref component.ActionLightningStormEntity, component.ActionLightningStorm, component: actions);
            _actions.AddAction(uid, ref component.ActionBroadcastEntity, component.ActionBroadcast, component: actions);
        }
        else if (phase == ShadowlingPhases.FailedAscension)
        {
            _popup.PopupEntity("WHATTTTT", uid, uid, PopupType.MediumCaution);
        }
    }

    #endregion

    private void AddPostHatchActions(EntityUid uid, ShadowlingComponent component)
    {
        if (!TryComp(uid, out ActionsComponent? actions))
            return;
        // Le Comps
        AddComp<ShadowlingGlareComponent>(uid);
        AddComp<ShadowlingEnthrallComponent>(uid);
        AddComp<ShadowlingVeilComponent>(uid);
        AddComp<ShadowlingRapidRehatchComponent>(uid);
        AddComp<ShadowlingShadowWalkComponent>(uid);
        AddComp<ShadowlingIcyVeinsComponent>(uid);
        AddComp<ShadowlingDestroyEnginesComponent>(uid);
        AddComp<ShadowlingCollectiveMindComponent>(uid);

        AddComp<ShadowlingAscendanceComponent>(uid); // remove this once debugged

        _actions.AddAction(uid, ref component.ActionGlareEntity, component.ActionGlare, component: actions);
        _actions.AddAction(uid, ref component.ActionEnthrallEntity, component.ActionEnthrall, component: actions);
        _actions.AddAction(uid, ref component.ActionVeilEntity, component.ActionVeil, component: actions);
        _actions.AddAction(uid, ref component.ActionRapidRehatchEntity, component.ActionRapidRehatch, component: actions);
        _actions.AddAction(uid, ref component.ActionShadowWalkEntity, component.ActionShadowWalk, component: actions);
        _actions.AddAction(uid, ref component.ActionIcyVeinsEntity, component.ActionIcyVeins, component: actions);
        _actions.AddAction(uid, ref component.ActionDestroyEnginesEntity, component.ActionDestroyEngines, component: actions);
        _actions.AddAction(uid, ref component.ActionCollectiveMindEntity, component.ActionCollectiveMind, component: actions);
        _actions.AddAction(uid, ref component.ActionAscendanceEntity, component.ActionAscendance, component: actions);
    }

    public bool CanEnthrall(EntityUid uid, EntityUid target)
    {
        if (HasComp<ShadowlingComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("shadowling-enthrall-shadowling"), uid, uid, PopupType.SmallCaution);
            return false;
        }

        if (HasComp<ThrallComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("shadowling-enthrall-already-thrall"), uid, uid, PopupType.SmallCaution);
            return false;
        }

        if (!HasComp<HumanoidAppearanceComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("shadowling-enthrall-non-humanoid"), uid, uid, PopupType.SmallCaution);
            return false;
        }

        // Psionic interaction
        if (HasComp<PsionicInsulationComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("shadowling-enthrall-psionic-insulated"), uid, uid, PopupType.SmallCaution);
            return false;
        }

        // Target needs to be alive
        if (TryComp<MobStateComponent>(target, out var mobState))
        {
            if (_mobStateSystem.IsCritical(target, mobState) || _mobStateSystem.IsCritical(target, mobState))
            {
                _popup.PopupEntity(Loc.GetString("shadowling-enthrall-dead"), uid, uid, PopupType.SmallCaution);
                return false;
            }
        }

        return true;
    }

    public bool CanGlare(EntityUid target)
    {
        if (!HasComp<MobStateComponent>(target))
            return false;

        if (HasComp<ShadowlingComponent>(target))
            return false;

        if (HasComp<ThrallComponent>(target))
            return false;

        return true;
    }

    public void DoEnthrall(EntityUid uid, SimpleDoAfterEvent args)
    {
        if (args.Cancelled)
            return;
        if (args.Args.Target is null)
            return;

        var target = args.Args.Target.Value;

        var thrall = EnsureComp<ThrallComponent>(target);

        if (TryComp<ShadowlingComponent>(uid, out var sling))
        {
            sling.Thralls.Add(target);
            thrall.Converter = uid;

            OnThrallAdded(uid, target, sling);
        }
    }
}
