using System.Linq;
using Content.Server.Construction.Components;
using Content.Server.Storage.EntitySystems;
using Content.Shared.DoAfter;
using Content.Shared.Construction.Components;
using Content.Shared.Exchanger;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Storage;
using Robust.Shared.Containers;
using Robust.Shared.Utility;
using Content.Shared.Wires;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Collections;

namespace Content.Server.Construction;

public sealed class PartExchangerSystem : EntitySystem
{
    [Dependency] private readonly ConstructionSystem _construction = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly StorageSystem _storage = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<PartExchangerComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<PartExchangerComponent, ExchangerDoAfterEvent>(OnDoAfter);
    }

    private void OnDoAfter(EntityUid uid, PartExchangerComponent component, DoAfterEvent args)
    {
        if (args.Cancelled)
        {
            component.AudioStream = _audio.Stop(component.AudioStream);
            return;
        }

        if (args.Handled || args.Args.Target == null || !TryComp<StorageComponent>(uid, out var storage))
            return;

        var machinePartQuery = GetEntityQuery<MachinePartComponent>();
        var machineParts = new List<Entity<MachinePartComponent>>();

        foreach (var item in storage.Container.ContainedEntities) //get parts in RPED
        {
            if (machinePartQuery.TryGetComponent(item, out var part))
                machineParts.Add((item, part));
        }

        TryExchangeMachineParts(args.Args.Target.Value, uid, machineParts);
        TryConstructMachineParts(args.Args.Target.Value, uid, machineParts);

        args.Handled = true;
    }

    private void TryExchangeMachineParts(EntityUid uid, EntityUid storageUid, List<Entity<MachinePartComponent>> machineParts)
    {
        if (!TryComp<MachineComponent>(uid, out var machine))
            return;

        var machinePartQuery = GetEntityQuery<MachinePartComponent>();
        var board = machine.BoardContainer.ContainedEntities.FirstOrNull();

        if (!TryComp<MachineBoardComponent>(board, out var macBoardComp))
            return;

        foreach (var item in new ValueList<EntityUid>(machine.PartContainer.ContainedEntities)) //clone so don't modify during enumeration
        {
            if (!machinePartQuery.TryGetComponent(item, out var part))
                continue;

            machineParts.Add((item, part));
            _container.RemoveEntity(uid, item);
        }

        machineParts.Sort((x, y) => x.Comp.Rating.CompareTo(y.Comp.Rating));

        var updatedParts = new List<Entity<MachinePartComponent>>();
        foreach (var (machinePartId, amount) in macBoardComp.MachinePartRequirements)
        {
            var target = machineParts.Where(p => p.Comp.PartType == machinePartId).Take(amount);
            updatedParts.AddRange(target);
        }

        foreach (var part in updatedParts)
        {
            _container.Insert(part.Owner, machine.PartContainer);
            machineParts.Remove(part);
        }

        //put the unused parts back into rped. (this also does the "swapping")
        foreach (var (unused, _) in machineParts)
            _storage.Insert(storageUid, unused, out _, playSound: false);

        _construction.RefreshParts(uid, machine);
    }

    private void TryConstructMachineParts(EntityUid uid, EntityUid storageEnt, List<Entity<MachinePartComponent>> machineParts)
    {
        if (!TryComp<MachineFrameComponent>(uid, out var machine))
            return;

        var machinePartQuery = GetEntityQuery<MachinePartComponent>();
        var board = machine.BoardContainer.ContainedEntities.FirstOrNull();

        if (!TryComp<MachineBoardComponent>(board, out var macBoardComp))
            return;

        foreach (var item in new ValueList<EntityUid>(machine.PartContainer.ContainedEntities)) //clone so don't modify during enumeration
        {
            if (!machinePartQuery.TryGetComponent(item, out var part))
                continue;

            machineParts.Add((item, part));
            _container.RemoveEntity(uid, item);
            machine.MachinePartProgress[part.PartType]--;
        }

        machineParts.Sort((x, y) => y.Comp.Rating.CompareTo(x.Comp.Rating));

        var updatedParts = new List<Entity<MachinePartComponent>>();
        foreach (var (machinePartId, amount) in macBoardComp.MachinePartRequirements)
        {
            var target = machineParts.Where(p => p.Comp.PartType == machinePartId).Take(amount);
            updatedParts.AddRange(target);
        }

        foreach (var pair in updatedParts)
        {
            if (!machine.MachinePartRequirements.ContainsKey(pair.Comp.PartType))
                continue;

            _container.Insert(pair.Owner, machine.PartContainer);
            machine.MachinePartProgress[pair.Comp.PartType]++;
            machineParts.Remove(pair);
        }

        //put the unused parts back into rped. (this also does the "swapping")
        foreach (var (unused, _) in machineParts)
            _storage.Insert(storageEnt, unused, out _, playSound: false);
    }

    private void OnAfterInteract(EntityUid uid, PartExchangerComponent component, AfterInteractEvent args)
    {
        if (component.DoDistanceCheck && !args.CanReach
            || args.Target == null
            || !HasComp<MachineComponent>(args.Target) && !HasComp<MachineFrameComponent>(args.Target))
            return;

        if (TryComp<WiresPanelComponent>(args.Target, out var panel) && !panel.Open)
        {
            _popup.PopupEntity(Loc.GetString("construction-step-condition-wire-panel-open"), args.Target.Value);
            return;
        }

        component.AudioStream = _audio.PlayPvs(component.ExchangeSound, uid)?.Entity;

        var doAfter = new DoAfterArgs(
            EntityManager,
            args.User,
            component.ExchangeDuration,
            new ExchangerDoAfterEvent(),
            uid,
            target: args.Target,
            used: uid)
        {
            BreakOnDamage = true,
            BreakOnMove = true
        };

        _doAfter.TryStartDoAfter(doAfter);
    }
}
