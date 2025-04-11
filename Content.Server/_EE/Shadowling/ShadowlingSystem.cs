using Content.Server.Actions;
using Content.Server.Antag;
using Content.Server.Armor;
using Content.Server.Database.Migrations.Postgres;
using Content.Server.Destructible;
using Content.Server.DoAfter;
using Content.Server.Hands.Systems;
using Content.Server.Humanoid;
using Content.Server.Popups;
using Content.Server.Psionics;
using Content.Server.Roles;
using Content.Server.Stealth;
using Content.Server.Storage.EntitySystems;
using Content.Server.Strip;
using Content.Server.Stunnable;
using Content.Server.Traits;
using Content.Shared._EE.Clothing.Components;
using Content.Shared._EE.Shadowling.Systems;
using Content.Shared._EE.Shadowling;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Armor;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Damage;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Mindshield.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Content.Shared.Strip;
using Content.Shared.Timing;
using MathNet.Numerics.Distributions;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;


namespace Content.Server._EE.Shadowling;


/// <summary>
/// This handles the Shadowling's System
/// </summary>
public sealed partial class ShadowlingSystem : SharedShadowlingSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly HumanoidAppearanceSystem _humanoid = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;
    [Dependency] private readonly StealthSystem _stealth = default!;
    [Dependency] private readonly EntityStorageSystem _entityStorage = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly UseDelaySystem _delay = default!;
    [Dependency] private readonly StatusEffectsSystem _effects = default!;

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
        foreach (var action in comp.PostHatchShadowlingActions)
            _actions.AddAction(uid, action);
    }

    private bool CanEnthrall(EntityUid uid, EntityUid target)
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

    private bool CanGlare(EntityUid target)
    {
        if (HasComp<ShadowlingComponent>(target))
            return false;

        if (HasComp<ThrallComponent>(target))
            return false;

        return true;
    }
}
