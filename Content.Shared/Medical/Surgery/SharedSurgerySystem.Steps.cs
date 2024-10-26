using Content.Shared.Medical.Surgery.Conditions;
using Content.Shared.Medical.Surgery.Effects.Step;
using Content.Shared.Medical.Surgery.Steps;
using Content.Shared.Medical.Surgery.Tools;
//using Content.Shared._RMC14.Xenonids.Parasite;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Body.Organ;
using Content.Shared.Bed.Sleep;
using Content.Shared.Body.Events;
using Content.Shared.Buckle.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Mood;
using Content.Shared.Inventory;
using Content.Shared.Item;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using System.Linq;
using Robust.Shared.Toolshed.TypeParsers;

namespace Content.Shared.Medical.Surgery;

public abstract partial class SharedSurgerySystem
{
    private static readonly string[] BruteDamageTypes = { "Slash", "Blunt", "Piercing" };
    private static readonly string[] BurnDamageTypes = { "Heat", "Shock", "Cold", "Caustic" };
    private void InitializeSteps()
    {
        SubscribeLocalEvent<SurgeryStepComponent, SurgeryStepEvent>(OnToolStep);
        SubscribeLocalEvent<SurgeryStepComponent, SurgeryStepCompleteCheckEvent>(OnToolCheck);
        SubscribeLocalEvent<SurgeryStepComponent, SurgeryCanPerformStepEvent>(OnToolCanPerform);

        //SubSurgery<SurgeryCutLarvaRootsStepComponent>(OnCutLarvaRootsStep, OnCutLarvaRootsCheck);
        SubSurgery<SurgeryTendWoundsEffectComponent>(OnTendWoundsStep, OnTendWoundsCheck);
        SubSurgery<SurgeryStepCavityEffectComponent>(OnCavityStep, OnCavityCheck);
        SubSurgery<SurgeryAddPartStepComponent>(OnAddPartStep, OnAddPartCheck);
        SubSurgery<SurgeryRemovePartStepComponent>(OnRemovePartStep, OnRemovePartCheck);
        SubSurgery<SurgeryAddOrganStepComponent>(OnAddOrganStep, OnAddOrganCheck);
        SubSurgery<SurgeryRemoveOrganStepComponent>(OnRemoveOrganStep, OnRemoveOrganCheck);
        Subs.BuiEvents<SurgeryTargetComponent>(SurgeryUIKey.Key, subs =>
        {
            subs.Event<SurgeryStepChosenBuiMsg>(OnSurgeryTargetStepChosen);
        });
    }

    private void SubSurgery<TComp>(EntityEventRefHandler<TComp, SurgeryStepEvent> onStep,
        EntityEventRefHandler<TComp, SurgeryStepCompleteCheckEvent> onComplete) where TComp : IComponent
    {
        SubscribeLocalEvent(onStep);
        SubscribeLocalEvent(onComplete);
    }

    private void OnToolStep(Entity<SurgeryStepComponent> ent, ref SurgeryStepEvent args)
    {
        if (ent.Comp.Tool != null)
        {
            foreach (var reg in ent.Comp.Tool.Values)
            {
                if (!AnyHaveComp(args.Tools, reg.Component, out var tool))
                    return;

                if (_net.IsServer &&
                    TryComp(tool, out SurgeryToolComponent? toolComp) &&
                    toolComp.EndSound != null)
                {
                    _audio.PlayEntity(toolComp.EndSound, args.User, tool);
                }
            }
        }

        if (ent.Comp.Add != null)
        {
            foreach (var reg in ent.Comp.Add.Values)
            {
                var compType = reg.Component.GetType();
                if (HasComp(args.Part, compType))
                    continue;

                AddComp(args.Part, _compFactory.GetComponent(compType));
            }
        }

        if (ent.Comp.Remove != null)
        {
            foreach (var reg in ent.Comp.Remove.Values)
            {
                RemComp(args.Part, reg.Component.GetType());
            }
        }

        if (ent.Comp.BodyRemove != null)
        {
            foreach (var reg in ent.Comp.BodyRemove.Values)
            {
                RemComp(args.Body, reg.Component.GetType());
            }
        }

        if (!HasComp<ForcedSleepingComponent>(args.Body))
            RaiseLocalEvent(args.Body, new MoodEffectEvent("SurgeryPain"));

        if (!_inventory.TryGetSlotEntity(args.User, "gloves", out var gloves)
        || !_inventory.TryGetSlotEntity(args.User, "mask", out var mask))
        {
            var sepsis = new DamageSpecifier(_prototypes.Index<DamageTypePrototype>("Poison"), 5);
            var ev = new SurgeryStepDamageEvent(args.User, args.Body, args.Part, args.Surgery, sepsis, 0.5f);
            RaiseLocalEvent(args.Body, ref ev);
        }
    }

    private void OnToolCheck(Entity<SurgeryStepComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        if (ent.Comp.Add != null)
        {
            foreach (var reg in ent.Comp.Add.Values)
            {
                if (!HasComp(args.Part, reg.Component.GetType()))
                {
                    args.Cancelled = true;
                    return;
                }
            }
        }

        if (ent.Comp.Remove != null)
        {
            foreach (var reg in ent.Comp.Remove.Values)
            {
                if (HasComp(args.Part, reg.Component.GetType()))
                {
                    args.Cancelled = true;
                    return;
                }
            }
        }

        if (ent.Comp.BodyRemove != null)
        {
            foreach (var reg in ent.Comp.BodyRemove.Values)
            {
                if (HasComp(args.Body, reg.Component.GetType()))
                {
                    args.Cancelled = true;
                    return;
                }
            }
        }
    }

    private void OnToolCanPerform(Entity<SurgeryStepComponent> ent, ref SurgeryCanPerformStepEvent args)
    {
        if (HasComp<SurgeryOperatingTableConditionComponent>(ent))
        {
            if (!TryComp(args.Body, out BuckleComponent? buckle) ||
                !HasComp<OperatingTableComponent>(buckle.BuckledTo))
            {
                args.Invalid = StepInvalidReason.NeedsOperatingTable;
                return;
            }
        }

        RaiseLocalEvent(args.Body, ref args);

        if (args.Invalid != StepInvalidReason.None)
            return;

        if (ent.Comp.Tool != null)
        {
            args.ValidTools ??= new HashSet<EntityUid>();

            foreach (var reg in ent.Comp.Tool.Values)
            {
                if (!AnyHaveComp(args.Tools, reg.Component, out var withComp))
                {
                    args.Invalid = StepInvalidReason.MissingTool;

                    if (reg.Component is ISurgeryToolComponent tool)
                        args.Popup = $"You need {tool.ToolName} to perform this step!";

                    return;
                }

                args.ValidTools.Add(withComp);
            }
        }
    }

    private EntProtoId? GetProtoId(EntityUid entityUid)
    {
        if (!TryComp<MetaDataComponent>(entityUid, out var metaData))
            return null;

        return metaData.EntityPrototype?.ID;
    }

    private void OnTendWoundsStep(Entity<SurgeryTendWoundsEffectComponent> ent, ref SurgeryStepEvent args)
    {
        var group = ent.Comp.MainGroup == "Brute" ? BruteDamageTypes : BurnDamageTypes;

        if (!TryComp(args.Body, out DamageableComponent? damageable)
            || !group.Any(damageType => damageable.Damage.DamageDict.TryGetValue(damageType, out var value)
                && value > 0)
            && (!TryComp(args.Part, out BodyPartComponent? bodyPart)
            || bodyPart.Integrity == 100))
            return;

        var bonus = ent.Comp.HealMultiplier * damageable.DamagePerGroup[ent.Comp.MainGroup];
        if (_mobState.IsDead(args.Body))
            bonus *= 0.2;

        var adjustedDamage = new DamageSpecifier(ent.Comp.Damage);
        var bonusPerType = bonus / group.Length;

        foreach (var type in group)
        {
            adjustedDamage.DamageDict[type] -= bonusPerType;
        }

        var ev = new SurgeryStepDamageEvent(args.User, args.Body, args.Part, args.Surgery, adjustedDamage, 0.5f);
        RaiseLocalEvent(args.Body, ref ev);

        if (ent.Comp.IsAutoRepeatable)
        {
            var stepProto = GetProtoId(ent);
            var surgeryProto = GetProtoId(args.Surgery);
            if (stepProto != null && surgeryProto != null)
                CheckAndStartStep(args.User, args.Body, args.Part, ent, args.Surgery, stepProto.Value, surgeryProto.Value);
        }
    }

    private void OnTendWoundsCheck(Entity<SurgeryTendWoundsEffectComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        var group = ent.Comp.MainGroup == "Brute" ? BruteDamageTypes : BurnDamageTypes;

        if (!TryComp(args.Body, out DamageableComponent? damageable)
            || group.Any(damageType => damageable.Damage.DamageDict.TryGetValue(damageType, out var value)
                && value > 0)
            || !TryComp(args.Part, out BodyPartComponent? bodyPart)
            || bodyPart.Integrity < 100)
            args.Cancelled = true;
    }

    /*private void OnCutLarvaRootsStep(Entity<SurgeryCutLarvaRootsStepComponent> ent, ref SurgeryStepEvent args)
    {
        if (TryComp(args.Body, out VictimInfectedComponent? infected) &&
            infected.BurstAt > _timing.CurTime &&
            infected.SpawnedLarva == null)
        {
            infected.RootsCut = true;
        }
    }

    private void OnCutLarvaRootsCheck(Entity<SurgeryCutLarvaRootsStepComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        if (!TryComp(args.Body, out VictimInfectedComponent? infected) || !infected.RootsCut)
            args.Cancelled = true;

        // The larva has fully developed and surgery is now impossible
        // TODO: Surgery should still be possible, but the fully developed larva should escape while also saving the hosts life
        if (infected != null && infected.SpawnedLarva != null)
            args.Cancelled = true;
    }*/

    private void OnCavityStep(Entity<SurgeryStepCavityEffectComponent> ent, ref SurgeryStepEvent args)
    {
        if (!TryComp(args.Part, out BodyPartComponent? partComp) || partComp.PartType != BodyPartType.Torso)
            return;

        var activeHandEntity = _hands.EnumerateHeld(args.User).FirstOrDefault();
        if (activeHandEntity != default
            && ent.Comp.Action == "Insert"
            && TryComp<ItemComponent>(activeHandEntity, out ItemComponent? itemComp)
            && (itemComp.Size.Id == "Tiny"
            || itemComp.Size.Id == "Small"))
            _itemSlotsSystem.TryInsert(ent, partComp.ItemInsertionSlot, activeHandEntity, args.User);
        else if (ent.Comp.Action == "Remove")
            _itemSlotsSystem.TryEjectToHands(ent, partComp.ItemInsertionSlot, args.User);
    }

    private void OnCavityCheck(Entity<SurgeryStepCavityEffectComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        if (!TryComp(args.Part, out BodyPartComponent? partComp)
            || ent.Comp.Action == "Insert"
            && !partComp.ItemInsertionSlot.HasItem
            || ent.Comp.Action == "Remove"
            && partComp.ItemInsertionSlot.HasItem)
            args.Cancelled = true;
    }

    private void OnAddPartStep(Entity<SurgeryAddPartStepComponent> ent, ref SurgeryStepEvent args)
    {
        if (!TryComp(args.Surgery, out SurgeryPartRemovedConditionComponent? removedComp))
            return;

        foreach (var tool in args.Tools)
        {
            if (TryComp(tool, out BodyPartComponent? partComp)
                && partComp.PartType == removedComp.Part
                && (removedComp.Symmetry == null || partComp.Symmetry == removedComp.Symmetry))
            {
                var slotName = removedComp.Symmetry != null
                    ? $"{removedComp.Symmetry?.ToString().ToLower()} {removedComp.Part.ToString().ToLower()}"
                    : removedComp.Part.ToString().ToLower();
                _body.AttachPart(args.Part, slotName, tool);
            }
        }
    }

    private void OnAddPartCheck(Entity<SurgeryAddPartStepComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        if (!TryComp(args.Surgery, out SurgeryPartRemovedConditionComponent? removedComp)
            || !_body.GetBodyChildrenOfType(args.Body, removedComp.Part, symmetry: removedComp.Symmetry).Any())
            args.Cancelled = true;
    }

    private void OnRemovePartStep(Entity<SurgeryRemovePartStepComponent> ent, ref SurgeryStepEvent args)
    {
        if (!TryComp(args.Part, out BodyPartComponent? partComp)
            || partComp.Body != args.Body)
            return;

        var ev = new AmputateAttemptEvent(args.Part);
        RaiseLocalEvent(args.Part, ref ev);
        _hands.TryPickupAnyHand(args.User, args.Part);
    }

    private void OnRemovePartCheck(Entity<SurgeryRemovePartStepComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        if (!TryComp(args.Part, out BodyPartComponent? partComp)
            || partComp.Body == args.Body)
            args.Cancelled = true;
    }

    private void OnAddOrganStep(Entity<SurgeryAddOrganStepComponent> ent, ref SurgeryStepEvent args)
    {
        if (!TryComp(args.Part, out BodyPartComponent? partComp)
            || partComp.Body != args.Body
            || !TryComp(args.Surgery, out SurgeryOrganConditionComponent? organComp)
            || organComp.Organ == null)
            return;

        // Adding organs is generally done for a single one at a time, so we only need to check for the first.
        var firstOrgan = organComp.Organ.Values.FirstOrDefault();
        if (firstOrgan == default)
            return;

        foreach (var tool in args.Tools)
        {
            if (HasComp(tool, firstOrgan.Component.GetType()))
            {
                _body.AddOrganToFirstValidSlot(args.Part, tool, partComp);
            }
        }
    }

    private void OnAddOrganCheck(Entity<SurgeryAddOrganStepComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        if (!TryComp<SurgeryOrganConditionComponent>(args.Surgery, out var organComp)
            || organComp.Organ is null
            || !TryComp(args.Part, out BodyPartComponent? partComp)
            || partComp.Body != args.Body)
            return;

        // For now we naively assume that every entity will only have one of each organ type.
        // that we do surgery on, but in the future we'll need to reference their prototype somehow
        // to know if they need 2 hearts, 2 lungs, etc.
        foreach (var reg in organComp.Organ.Values)
        {
            if (!_body.TryGetBodyPartOrgans(args.Part, reg.Component.GetType(), out var _))
            {
                args.Cancelled = true;
            }
        }
    }

    private void OnRemoveOrganStep(Entity<SurgeryRemoveOrganStepComponent> ent, ref SurgeryStepEvent args)
    {
        if (!TryComp<SurgeryOrganConditionComponent>(args.Surgery, out var organComp)
            || organComp.Organ == null)
            return;

        foreach (var reg in organComp.Organ.Values)
        {
            _body.TryGetBodyPartOrgans(args.Part, reg.Component.GetType(), out var organs);
            if (organs != null && organs.Count > 0)
            {
                _body.RemoveOrgan(organs[0].Id, organs[0].Organ);
                _hands.TryPickupAnyHand(args.User, organs[0].Id);
            }
        }
    }

    private void OnRemoveOrganCheck(Entity<SurgeryRemoveOrganStepComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        if (!TryComp<SurgeryOrganConditionComponent>(args.Surgery, out var organComp)
            || organComp.Organ == null
            || !TryComp(args.Part, out BodyPartComponent? partComp)
            || partComp.Body != args.Body)
            return;

        foreach (var reg in organComp.Organ.Values)
        {
            if (_body.TryGetBodyPartOrgans(args.Part, reg.Component.GetType(), out var organs)
                && organs != null
                && organs.Count > 0)
            {
                args.Cancelled = true;
                return;
            }
        }
    }

    // Small duplicate for OnSurgeryTargetStepChosen, allows for continuously looping a given step.
    private void CheckAndStartStep(EntityUid user, EntityUid body, EntityUid part, EntityUid step, EntityUid surgery,
        EntProtoId stepProto, EntProtoId surgeryProto)
    {
        if (!CanPerformStep(user, body, part, step, true, out _, out _, out var validTools))
            return;

        if (_net.IsServer && validTools?.Count > 0)
        {
            foreach (var tool in validTools)
            {
                if (TryComp(tool, out SurgeryToolComponent? toolComp) &&
                    toolComp.EndSound != null)
                {
                    _audio.PlayEntity(toolComp.StartSound, user, tool);
                }
            }
        }

        if (TryComp(body, out TransformComponent? xform))
            _rotateToFace.TryFaceCoordinates(user, _transform.GetMapCoordinates(body, xform).Position);

        var ev = new SurgeryDoAfterEvent(surgeryProto, stepProto);
        // TODO: Make this serialized on a per surgery step basis, and also add penalties based on ghetto tools.
        var duration = 2f;
        if (TryComp(user, out SurgerySpeedModifierComponent? surgerySpeedMod))
            duration = duration / surgerySpeedMod.SpeedModifier;

        var doAfter = new DoAfterArgs(EntityManager, user, TimeSpan.FromSeconds(duration), ev, body, part)
        {
            BreakOnUserMove = true,
            BreakOnTargetMove = true,
        };
        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnSurgeryTargetStepChosen(Entity<SurgeryTargetComponent> ent, ref SurgeryStepChosenBuiMsg args)
    {
        if (args.Session.AttachedEntity is not { } user ||
            GetEntity(args.Entity) is not { Valid: true } body ||
            !IsSurgeryValid(body, GetEntity(args.Part), args.Surgery, args.Step, out var surgery, out var part, out var step))
            return;

        if (!PreviousStepsComplete(body, part, surgery, args.Step) ||
            IsStepComplete(body, part, args.Step, surgery))
            return;

        if (!CanPerformStep(user, body, part, step, true, out _, out _, out var validTools))
            return;

        if (_net.IsServer && validTools?.Count > 0)
        {
            foreach (var tool in validTools)
            {
                if (TryComp(tool, out SurgeryToolComponent? toolComp) &&
                    toolComp.EndSound != null)
                {
                    _audio.PlayEntity(toolComp.StartSound, user, tool);
                }
            }
        }

        if (TryComp(body, out TransformComponent? xform))
            _rotateToFace.TryFaceCoordinates(user, _transform.GetMapCoordinates(body, xform).Position);

        var ev = new SurgeryDoAfterEvent(args.Surgery, args.Step);
        // TODO: Make this serialized on a per surgery step basis, and also add penalties based on ghetto tools.
        var duration = 2f;
        if (TryComp(user, out SurgerySpeedModifierComponent? surgerySpeedMod))
            duration = duration / surgerySpeedMod.SpeedModifier;

        var doAfter = new DoAfterArgs(EntityManager, user, TimeSpan.FromSeconds(duration), ev, body, part)
        {
            BreakOnUserMove = true,
            BreakOnTargetMove = true,
        };
        _doAfter.TryStartDoAfter(doAfter);
    }

    private (Entity<SurgeryComponent> Surgery, int Step)? GetNextStep(EntityUid body, EntityUid part, Entity<SurgeryComponent?> surgery, List<EntityUid> requirements)
    {
        if (!Resolve(surgery, ref surgery.Comp))
            return null;

        if (requirements.Contains(surgery))
            throw new ArgumentException($"Surgery {surgery} has a requirement loop: {string.Join(", ", requirements)}");

        requirements.Add(surgery);

        if (surgery.Comp.Requirement is { } requirementId &&
            GetSingleton(requirementId) is { } requirement &&
            GetNextStep(body, part, requirement, requirements) is { } requiredNext)
        {
            return requiredNext;
        }

        for (var i = 0; i < surgery.Comp.Steps.Count; i++)
        {
            var surgeryStep = surgery.Comp.Steps[i];
            if (!IsStepComplete(body, part, surgeryStep, surgery))
                return ((surgery, surgery.Comp), i);
        }

        return null;
    }

    public (Entity<SurgeryComponent> Surgery, int Step)? GetNextStep(EntityUid body, EntityUid part, EntityUid surgery)
    {
        return GetNextStep(body, part, surgery, new List<EntityUid>());
    }

    public bool PreviousStepsComplete(EntityUid body, EntityUid part, Entity<SurgeryComponent> surgery, EntProtoId step)
    {
        // TODO RMC14 use index instead of the prototype id
        if (surgery.Comp.Requirement is { } requirement)
        {
            if (GetSingleton(requirement) is not { } requiredEnt ||
                !TryComp(requiredEnt, out SurgeryComponent? requiredComp) ||
                !PreviousStepsComplete(body, part, (requiredEnt, requiredComp), step))
            {
                return false;
            }
        }

        foreach (var surgeryStep in surgery.Comp.Steps)
        {
            if (surgeryStep == step)
                break;

            if (!IsStepComplete(body, part, surgeryStep, surgery))
                return false;
        }

        return true;
    }

    public bool CanPerformStep(EntityUid user, EntityUid body, EntityUid part,
        EntityUid step, bool doPopup, out string? popup, out StepInvalidReason reason,
        out HashSet<EntityUid>? validTools)
    {
        var type = BodyPartType.Other;
        if (TryComp(part, out BodyPartComponent? partComp))
        {
            type = partComp.PartType;
        }

        var slot = type switch
        {
            BodyPartType.Head => SlotFlags.HEAD,
            BodyPartType.Torso => SlotFlags.OUTERCLOTHING | SlotFlags.INNERCLOTHING,
            BodyPartType.Arm => SlotFlags.OUTERCLOTHING | SlotFlags.INNERCLOTHING,
            BodyPartType.Hand => SlotFlags.GLOVES,
            BodyPartType.Leg => SlotFlags.OUTERCLOTHING | SlotFlags.LEGS,
            BodyPartType.Foot => SlotFlags.FEET,
            BodyPartType.Tail => SlotFlags.NONE,
            BodyPartType.Other => SlotFlags.NONE,
            _ => SlotFlags.NONE
        };

        var check = new SurgeryCanPerformStepEvent(user, body, GetTools(user), slot);
        RaiseLocalEvent(step, ref check);
        popup = check.Popup;
        validTools = check.ValidTools;

        if (check.Invalid != StepInvalidReason.None)
        {
            if (doPopup && check.Popup != null)
                _popup.PopupEntity(check.Popup, user, PopupType.SmallCaution);

            reason = check.Invalid;
            return false;
        }

        reason = default;
        return true;
    }

    public bool CanPerformStep(EntityUid user, EntityUid body, EntityUid part, EntityUid step, bool doPopup)
    {
        return CanPerformStep(user, body, part, step, doPopup, out _, out _, out _);
    }

    public bool IsStepComplete(EntityUid body, EntityUid part, EntProtoId step, EntityUid surgery)
    {
        if (GetSingleton(step) is not { } stepEnt)
            return false;

        var ev = new SurgeryStepCompleteCheckEvent(body, part, surgery);
        RaiseLocalEvent(stepEnt, ref ev);

        return !ev.Cancelled;
    }

    private bool AnyHaveComp(List<EntityUid> tools, IComponent component, out EntityUid withComp)
    {
        foreach (var tool in tools)
        {
            if (HasComp(tool, component.GetType()))
            {
                withComp = tool;
                return true;
            }
        }

        withComp = default;
        return false;
    }
}