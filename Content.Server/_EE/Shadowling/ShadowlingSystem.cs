using Content.Server.Actions;
using Content.Server.Armor;
using Content.Server.Database.Migrations.Postgres;
using Content.Server.Destructible;
using Content.Server.Hands.Systems;
using Content.Server.Humanoid;
using Content.Server.Popups;
using Content.Server.Stealth;
using Content.Server.Storage.EntitySystems;
using Content.Server.Strip;
using Content.Shared._EE.Clothing.Components;
using Content.Shared._EE.Shadowling.Systems;
using Content.Shared._EE.Shadowling;
using Content.Shared.Armor;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Damage;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Strip;
using MathNet.Numerics.Distributions;
using Robust.Shared.Timing;


namespace Content.Server._EE.Shadowling;


/// <summary>
/// This handles...
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
}
