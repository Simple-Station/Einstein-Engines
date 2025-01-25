using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Body.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Chemistry.Components;
using Content.Shared.Clothing;
using Content.Shared.Inventory.Events;
using Content.Shared.Inventory;
using Content.Server.Power.EntitySystems;
using Robust.Server.Containers;

namespace Content.Server.Body.Systems;

public sealed class LungSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
    [Dependency] private readonly InternalsSystem _internals = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;
    [Dependency] private readonly InventorySystem _inventory = default!; // Goobstation



    public static string LungSolutionName = "Lung";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LungComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<BreathToolComponent, ComponentInit>(OnBreathToolInit); // Goobstation - Modsuits - Update on component toggle
        SubscribeLocalEvent<BreathToolComponent, GotEquippedEvent>(OnGotEquipped);
        SubscribeLocalEvent<BreathToolComponent, GotUnequippedEvent>(OnGotUnequipped);
        SubscribeLocalEvent<BreathToolComponent, ItemMaskToggledEvent>(OnMaskToggled);
    }

    private void OnGotUnequipped(Entity<BreathToolComponent> ent, ref GotUnequippedEvent args)
    {
        _atmosphereSystem.DisconnectInternals(ent);
    }

    private void OnGotEquipped(Entity<BreathToolComponent> ent, ref GotEquippedEvent args)
    {
        if ((args.SlotFlags & ent.Comp.AllowedSlots) == 0)
        {
            return;
        }

        ent.Comp.IsFunctional = true;

        if (TryComp(args.Equipee, out InternalsComponent? internals))
        {
            ent.Comp.ConnectedInternalsEntity = args.Equipee;
            _internals.ConnectBreathTool((args.Equipee, internals), ent);
        }
    }

    private void OnComponentInit(Entity<LungComponent> entity, ref ComponentInit args)
    {
        if (_solutionContainerSystem.EnsureSolution(entity.Owner, entity.Comp.SolutionName, out var solution))
        {
            solution.MaxVolume = entity.Comp.MaxVolume;
            solution.CanReact = entity.Comp.CanReact;
        }
    }

    // Goobstation - Update component state on component toggle
    private void OnBreathToolInit(Entity<BreathToolComponent> ent, ref ComponentInit args)
    {
        var comp = ent.Comp;

        comp.IsFunctional = true;

        if (!_inventory.TryGetContainingEntity(ent.Owner, out var parent)
            || !_inventory.TryGetContainingSlot(ent.Owner, out var slot)
            || (slot.SlotFlags & comp.AllowedSlots) == 0
            || !TryComp(parent, out InternalsComponent? internals))
            return;

        ent.Comp.ConnectedInternalsEntity = parent;
        _internals.ConnectBreathTool((parent.Value, internals), ent);
    }


    private void OnMaskToggled(Entity<BreathToolComponent> ent, ref ItemMaskToggledEvent args)
    {
        if (args.IsToggled || args.IsEquip)
        {
            _atmosphereSystem.DisconnectInternals(ent);
        }
        else
        {
            ent.Comp.IsFunctional = true;

            if (TryComp(args.Wearer, out InternalsComponent? internals))
            {
                ent.Comp.ConnectedInternalsEntity = args.Wearer;
                _internals.ConnectBreathTool((args.Wearer, internals), ent);
            }
        }
    }

    public void GasToReagent(EntityUid uid, LungComponent lung)
    {
        if (!_solutionContainerSystem.ResolveSolution(uid, lung.SolutionName, ref lung.Solution, out var solution))
            return;

        GasToReagent(lung.Air, solution);
        _solutionContainerSystem.UpdateChemicals(lung.Solution.Value);
    }

    private void GasToReagent(GasMixture gas, Solution solution)
    {
        foreach (var gasId in Enum.GetValues<Gas>())
        {
            var i = (int) gasId;
            var moles = gas[i];
            if (moles <= 0)
                continue;

            var reagent = _atmosphereSystem.GasReagents[i];
            if (reagent is null)
                continue;

            var amount = moles * Atmospherics.BreathMolesToReagentMultiplier;
            solution.AddReagent(reagent, amount);
        }
    }

    public Solution GasToReagent(GasMixture gas)
    {
        var solution = new Solution();
        GasToReagent(gas, solution);
        return solution;
    }
}
