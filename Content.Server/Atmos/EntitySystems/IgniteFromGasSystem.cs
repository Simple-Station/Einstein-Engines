using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.Administration.Logs;
using Content.Server.Atmos.Components;
using Content.Server.Bed.Components;
using Content.Shared._Shitmed.Body.Events;
using Content.Shared.Alert;
using Content.Shared.Atmos;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.FixedPoint;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Mood;
using Robust.Shared.Containers;

namespace Content.Server.Atmos.EntitySystems;

public sealed class IgniteFromGasSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger= default!;
    [Dependency] private readonly InventorySystem _inventory = default!;

    private const float UpdateTimer = 1f;
    private float _timer;

    /// <summary>
    ///   Which clothing slots, when they have an item with IgniteFromGasImmunityComponent,
    ///   grant immunity to body parts.
    /// </summary>
    private readonly Dictionary<String, HashSet<BodyPartType>> ImmunitySlots = new() {
        ["head"] = new HashSet<BodyPartType> { BodyPartType.Head },
        ["jumpsuit"] = new HashSet<BodyPartType> {
            BodyPartType.Other,
            BodyPartType.Torso,
            BodyPartType.Arm,
            BodyPartType.Hand,
            BodyPartType.Leg,
            BodyPartType.Foot,
            BodyPartType.Tail,
        },
        ["outerClothing"] = new HashSet<BodyPartType> {
            BodyPartType.Other,
            BodyPartType.Torso,
            BodyPartType.Arm,
            BodyPartType.Hand,
            BodyPartType.Leg,
            BodyPartType.Foot,
            BodyPartType.Tail,
        },
        ["gloves"] = new HashSet<BodyPartType> { BodyPartType.Hand, },
        ["shoes"] = new HashSet<BodyPartType> { BodyPartType.Foot, },
    };

    public override void Initialize()
    {
        SubscribeLocalEvent<FlammableComponent, BodyPartAddedEvent>(OnBodyPartAdded);
        SubscribeLocalEvent<FlammableComponent, BodyPartAttachedEvent>(OnBodyPartAttached);

        SubscribeLocalEvent<IgniteFromGasComponent, BodyPartRemovedEvent>(OnBodyPartRemoved);
        SubscribeLocalEvent<IgniteFromGasComponent, BodyPartDroppedEvent>(OnBodyPartDropped);

        SubscribeLocalEvent<IgniteFromGasImmunityComponent, GotEquippedEvent>(OnIgniteFromGasImmunityEquipped);
        SubscribeLocalEvent<IgniteFromGasImmunityComponent, GotUnequippedEvent>(OnIgniteFromGasImmunityUnequipped);
    }

    private void OnBodyPartAdded(EntityUid uid, FlammableComponent component, BodyPartAddedEvent args)
    {
        HandleAddBodyPart(uid, args.Part.Owner, args.Part.Comp);
    }

    private void OnBodyPartAttached(EntityUid uid, FlammableComponent component, BodyPartAttachedEvent args)
    {
        HandleAddBodyPart(component.Owner, args.Part.Owner, args.Part.Comp);
    }

    private void HandleAddBodyPart(EntityUid uid, EntityUid partUid, BodyPartComponent comp)
    {
        if (!TryComp<IgniteFromGasPartComponent>(partUid, out var ignitePart))
            return;

        if (!TryComp<IgniteFromGasComponent>(uid, out var ignite))
        {
            ignite = new IgniteFromGasComponent{
                Gas = ignitePart.Gas,
                IgnitableBodyParts = new Dictionary<(BodyPartType, BodyPartSymmetry), float>()
                {
                    [(comp.PartType, comp.Symmetry)] = ignitePart.FireStacks
                }
            };

            AddComp(uid, ignite);
        }
        else
            ignite.IgnitableBodyParts[(comp.PartType, comp.Symmetry)] = ignitePart.FireStacks;

        UpdateIgniteImmunity(uid, ignite);
    }

    private void OnBodyPartRemoved(EntityUid uid, IgniteFromGasComponent component, BodyPartRemovedEvent args)
    {
        HandleRemoveBodyPart(uid, args.Part.Owner, args.Part.Comp, component);
    }

    private void OnBodyPartDropped(EntityUid uid, IgniteFromGasComponent component, BodyPartDroppedEvent args)
    {
        HandleRemoveBodyPart(component.Owner, args.Part.Owner, args.Part.Comp, component);
    }

    private void HandleRemoveBodyPart(EntityUid uid, EntityUid partUid, BodyPartComponent part, IgniteFromGasComponent ignite)
    {
        if (!TryComp<IgniteFromGasPartComponent>(partUid, out var ignitePart))
            return;

        ignite.IgnitableBodyParts.Remove((part.PartType, part.Symmetry));

        if (ignite.IgnitableBodyParts.Count == 0)
        {
            RemCompDeferred<IgniteFromGasComponent>(uid);
            return;
        }

        UpdateIgniteImmunity(uid, ignite);
    }

    private void OnIgniteFromGasImmunityEquipped(EntityUid uid, IgniteFromGasImmunityComponent igniteImmunity, GotEquippedEvent args)
    {
        if (TryComp<IgniteFromGasComponent>(args.Equipee, out var ignite) && ImmunitySlots.ContainsKey(args.Slot))
            UpdateIgniteImmunity(args.Equipee, ignite);
    }

    private void OnIgniteFromGasImmunityUnequipped(EntityUid uid, IgniteFromGasImmunityComponent igniteImmunity, GotUnequippedEvent args)
    {
        if (TryComp<IgniteFromGasComponent>(args.Equipee, out var ignite) && ImmunitySlots.ContainsKey(args.Slot))
            UpdateIgniteImmunity(args.Equipee, ignite);
    }

    public void UpdateIgniteImmunity(EntityUid uid, IgniteFromGasComponent? ignite = null, InventoryComponent? inv = null, ContainerManagerComponent? contMan = null)
    {
        if (!Resolve(uid, ref ignite, ref inv, ref contMan))
            return;

        var exposedBodyParts = new Dictionary<(BodyPartType, BodyPartSymmetry), float>(ignite.IgnitableBodyParts);

        // This is O(n^2) but I don't think it matters
        // TODO: use TargetBodyPart instead of a tuple for these
        foreach (var (slot, protectedBodyParts) in ImmunitySlots.Select(s => (s.Key, s.Value)))
        {
            if (!_inventory.TryGetSlotEntity(uid, slot, out var equipment, inv, contMan) ||
                !HasComp<IgniteFromGasImmunityComponent>(equipment))
                continue;

            foreach (var protectedBodyPart in protectedBodyParts)
            {
                exposedBodyParts.Remove((protectedBodyPart, BodyPartSymmetry.Left));
                exposedBodyParts.Remove((protectedBodyPart, BodyPartSymmetry.Right));
                exposedBodyParts.Remove((protectedBodyPart, BodyPartSymmetry.None));
            }
        }

        if (exposedBodyParts.Count() == 0)
        {
            ignite.HasImmunity = true;
            ignite.FireStacks = 0;
            return;
        }

        ignite.HasImmunity = false;
        var exposedFireStacks = 0f;
        foreach (var fireStacks in exposedBodyParts.Values)
            exposedFireStacks += fireStacks;
        ignite.FireStacks = ignite.BaseFireStacks + exposedFireStacks;
    }

    public override void Update(float frameTime)
    {
        _timer += frameTime;

        if (_timer < UpdateTimer)
            return;

        _timer -= UpdateTimer;

        var enumerator = EntityQueryEnumerator<IgniteFromGasComponent, FlammableComponent>();
        while (enumerator.MoveNext(out var uid, out var ignite, out var flammable))
        {
            if (ignite.HasImmunity || HasComp<InStasisComponent>(uid))
                continue;

            var gas = _atmos.GetContainingMixture(uid, excite: true);

            if (gas is null || gas[(int) ignite.Gas] < ignite.MolesToIgnite)
                continue;

            _flammable.AdjustFireStacks(uid, ignite.FireStacks, flammable);
            _flammable.Ignite(uid, uid, flammable, ignoreFireProtection: true);
        }
    }
}
