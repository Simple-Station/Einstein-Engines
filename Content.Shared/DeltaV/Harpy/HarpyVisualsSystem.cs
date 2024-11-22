using Content.Shared.Inventory.Events;
using Content.Shared.Tag;
using Content.Shared.Humanoid;

namespace Content.Shared.DeltaV.Harpy;

public sealed class HarpyVisualsSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tagSystem = default!;
    [Dependency] private readonly SharedHumanoidAppearanceSystem _humanoidSystem = default!;

    [ValidatePrototypeId<TagPrototype>]
    private const string HarpyWingsTag = "HidesHarpyWings";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<Traits.Assorted.Components.SingerComponent, DidEquipEvent>(OnDidEquipEvent);
        SubscribeLocalEvent<Traits.Assorted.Components.SingerComponent, DidUnequipEvent>(OnDidUnequipEvent);
    }

    private void OnDidEquipEvent(EntityUid uid, Traits.Assorted.Components.SingerComponent component, DidEquipEvent args)
    {
        if (args.Slot == "outerClothing" && _tagSystem.HasTag(args.Equipment, HarpyWingsTag))
        {
            _humanoidSystem.SetLayerVisibility(uid, HumanoidVisualLayers.RArm, false);
            _humanoidSystem.SetLayerVisibility(uid, HumanoidVisualLayers.Tail, false);
        }
    }

    private void OnDidUnequipEvent(EntityUid uid, Traits.Assorted.Components.SingerComponent component, DidUnequipEvent args)
    {
        if (args.Slot == "outerClothing" && _tagSystem.HasTag(args.Equipment, HarpyWingsTag))
        {
            _humanoidSystem.SetLayerVisibility(uid, HumanoidVisualLayers.RArm, true);
            _humanoidSystem.SetLayerVisibility(uid, HumanoidVisualLayers.Tail, true);
        }
    }
}
