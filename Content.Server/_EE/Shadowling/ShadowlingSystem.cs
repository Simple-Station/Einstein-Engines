using Content.Server.Actions;
using Content.Server.Popups;
using Content.Server.Shuttles.Events;
using Content.Server.Storage.EntitySystems;
using Content.Shared._EE.Shadowling.Systems;
using Content.Shared._EE.Shadowling;
using Content.Shared.Abilities.Psionics;
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

        SubscribeAbilities();
    }

    #region Event Handlers

    private void OnInit(EntityUid uid, ShadowlingComponent comp, ref ComponentInit args)
    {
        foreach (var actionId in comp.BaseShadowlingActions)
            _actions.AddAction(uid, actionId);
    }

    private void BeforeDamageChanged(EntityUid uid, ShadowlingComponent comp, BeforeDamageChangedEvent args)
    {
        if (comp.IsHatching)
            args.Cancelled = true;

        // Can't take damage during hatching
    }

    #endregion

    private void AddPostHatchActions(EntityUid uid, ShadowlingComponent comp)
    {
        AddComp<ShadowlingGlareComponent>(uid);
        AddComp<ShadowlingEnthrallComponent>(uid);
        AddComp<ShadowlingVeilComponent>(uid);
        AddComp<ShadowlingRapidRehatchComponent>(uid);
        AddComp<ShadowlingShadowWalkComponent>(uid);
        AddComp<ShadowlingIcyVeinsComponent>(uid);
        AddComp<ShadowlingDestroyEnginesComponent>(uid);

        foreach (var action in comp.PostHatchShadowlingActions)
            _actions.AddAction(uid, action);
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
