using Content.Server.Actions;
using Content.Server.Popups;
using Content.Server.Storage.EntitySystems;
using Content.Shared._EE.Shadowling.Systems;
using Content.Shared._EE.Shadowling;
using Content.Shared._EE.Shadowling.Components;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Humanoid;
using Content.Shared.Inventory;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;


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

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingComponent, ComponentInit>(OnInit);

        SubscribeLocalEvent<ShadowlingComponent, BeforeDamageChangedEvent>(BeforeDamageChanged);
        SubscribeLocalEvent<ShadowlingComponent, PhaseChangedEvent>(OnPhaseChanged);

        SubscribeAbilities();
    }

    #region Event Handlers

    private void OnInit(EntityUid uid, ShadowlingComponent component, ref ComponentInit args)
    {
        if (!TryComp(uid, out ActionsComponent? actions))
            return;

        _actions.AddAction(uid, ref component.ActionHatchEntity, component.ActionHatch, component: actions);
    }

    private void BeforeDamageChanged(EntityUid uid, ShadowlingComponent comp, BeforeDamageChangedEvent args)
    {
        if (comp.IsHatching)
            args.Cancelled = true;

        // Can't take damage during hatching
    }

    private void OnPhaseChanged(EntityUid uid, ShadowlingComponent component, PhaseChangedEvent args)
    {
        if (!TryComp<ActionsComponent>(uid, out var actions))
            return;

        if (args.Phase == ShadowlingPhases.PostHatch)
        {
            // _actions.RemoveAction();
            AddPostHatchActions(uid, component);
        }
        else if (args.Phase == ShadowlingPhases.Ascension)
        {
            // Remove all previous actions
            foreach (var action in actions.Actions)
            {
                if (!HasComp<ShadowlingActionComponent>(action))
                    continue;

                _actions.RemoveAction(uid, action);
            }

            // Add Ascension Actions and Components
            AddComp<ShadowlingAnnihilateComponent>(uid);
            AddComp<ShadowlingHypnosisComponent>(uid);
            _actions.AddAction(uid, ref component.ActionAnnihilateEntity, component.ActionAnnihilate, component: actions);
            _actions.AddAction(uid, ref component.ActionHypnosisEntity, component.ActionHypnosis, component: actions);
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
}
