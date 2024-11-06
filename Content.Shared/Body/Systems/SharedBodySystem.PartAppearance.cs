using System.Linq;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;

namespace Content.Shared.Body.Systems;
public partial class SharedBodySystem
{
    [Dependency] private readonly SharedHumanoidAppearanceSystem _humanoid = default!;
    private void InitializePartAppearances()
    {
        base.Initialize();

        SubscribeLocalEvent<BodyPartAppearanceComponent, ComponentStartup>(OnPartAppearanceStartup);
        SubscribeLocalEvent<BodyPartAppearanceComponent, AfterAutoHandleStateEvent>(HandleState);
        SubscribeLocalEvent<BodyComponent, BodyPartAttachedEvent>(OnPartAttachedToBody);
        SubscribeLocalEvent<BodyComponent, BodyPartDroppedEvent>(OnPartDroppedFromBody);
    }

    private void OnPartAppearanceStartup(EntityUid uid, BodyPartAppearanceComponent component, ComponentStartup args)
    {

        // God this function reeks, it needs some cleanup BADLY. Help is appreciated as always.

        if (!TryComp(uid, out BodyPartComponent? part)
            || part.OriginalBody == null
            || TerminatingOrDeleted(part.OriginalBody.Value)
            || !TryComp(part.OriginalBody.Value, out HumanoidAppearanceComponent? bodyAppearance)
            || HumanoidVisualLayersExtension.ToHumanoidLayers(part) is not { } relevantLayer)
            return;

        var customLayers = bodyAppearance.CustomBaseLayers;
        var spriteLayers = bodyAppearance.BaseLayers;
        component.Type = relevantLayer;
        component.OriginalBody = part.OriginalBody.Value;
        part.Sex = bodyAppearance.Sex;

        // Thanks Rane for the human reskin :)
        if (bodyAppearance.Species.ToString() == "Felinid")
            part.Species = "Human";
        else
            part.Species = bodyAppearance.Species;

        if (customLayers.ContainsKey(component.Type))
        {
            component.ID = customLayers[component.Type].Id;
            component.Color = customLayers[component.Type].Color;
        }
        else if (spriteLayers.ContainsKey(component.Type))
        {
            component.ID = spriteLayers[component.Type].ID;
            component.Color = bodyAppearance.SkinColor;
        }
        else
        {
            component.Color = bodyAppearance.SkinColor;
            var symmetryPrefix = part.Symmetry switch
            {
                BodyPartSymmetry.Left => "L",
                BodyPartSymmetry.Right => "R",
                _ => ""
            };

            var genderSuffix = "";

            if (part.PartType == BodyPartType.Torso || part.PartType == BodyPartType.Head)
                genderSuffix = part.Sex.ToString();

            component.ID = $"Mob{part.Species}{symmetryPrefix}{part.PartType}{genderSuffix}";
        }

        // I HATE HARDCODED CHECKS I HATE HARDCODED CHECKS I HATE HARDCODED CHECKS
        if (part.PartType == BodyPartType.Head)
            component.EyeColor = bodyAppearance.EyeColor;

        var markingsByLayer = new Dictionary<HumanoidVisualLayers, List<Marking>>();

        foreach (var layer in HumanoidVisualLayersExtension.Sublayers(relevantLayer))
        {
            var category = MarkingCategoriesConversion.FromHumanoidVisualLayers(layer);
            if (bodyAppearance.MarkingSet.Markings.TryGetValue(category, out var markingList))
                markingsByLayer[layer] = markingList.Select(m => new Marking(m.MarkingId, m.MarkingColors.ToList())).ToList();
        }

        component.Markings = markingsByLayer;
    }
    private void HandleState(EntityUid uid, BodyPartAppearanceComponent component, ref AfterAutoHandleStateEvent args)
    {
        ApplyPartMarkings(uid, component);
    }
    private void OnPartAttachedToBody(EntityUid uid, BodyComponent component, ref BodyPartAttachedEvent args)
    {
        if (!TryComp(args.Part, out BodyPartAppearanceComponent? partAppearance)
            || !TryComp(uid, out HumanoidAppearanceComponent? bodyAppearance))
            return;

        _humanoid.SetBaseLayerId(uid, partAppearance.Type, partAppearance.ID, sync: true, bodyAppearance);
        UpdateAppearance(uid, partAppearance);
    }

    private void OnPartDroppedFromBody(EntityUid uid, BodyComponent component, ref BodyPartDroppedEvent args)
    {
        if (TerminatingOrDeleted(uid)
            || !TryComp(args.Part, out BodyPartAppearanceComponent? appearance))
            return;

        RemoveAppearance(uid, appearance, args.Part);
    }

    protected void UpdateAppearance(EntityUid target,
        BodyPartAppearanceComponent component)
    {
        if (!TryComp(target, out HumanoidAppearanceComponent? bodyAppearance))
            return;

        if (component.EyeColor != null)
            bodyAppearance.EyeColor = component.EyeColor.Value;

        if (component.Color != null)
            _humanoid.SetBaseLayerColor(target, component.Type, component.Color, true, bodyAppearance);

        _humanoid.SetLayerVisibility(target, component.Type, true, true, bodyAppearance);
        foreach (var (visualLayer, markingList) in component.Markings)
        {
            _humanoid.SetLayerVisibility(target, visualLayer, true, true, bodyAppearance);
            foreach (var marking in markingList)
            {
                _humanoid.AddMarking(target, marking.MarkingId, marking.MarkingColors, false, true, bodyAppearance);
            }
        }
        Dirty(target, bodyAppearance);
    }

    protected void RemoveAppearance(EntityUid entity, BodyPartAppearanceComponent component, EntityUid partEntity)
    {
        if (!TryComp(entity, out HumanoidAppearanceComponent? bodyAppearance))
            return;

        foreach (var (visualLayer, markingList) in component.Markings)
        {
            _humanoid.SetLayerVisibility(entity, visualLayer, false, true, bodyAppearance);
        }
        RemovePartMarkings(entity, component, bodyAppearance);
    }

    protected abstract void ApplyPartMarkings(EntityUid target, BodyPartAppearanceComponent component);

    protected abstract void RemovePartMarkings(EntityUid target, BodyPartAppearanceComponent partAppearance, HumanoidAppearanceComponent bodyAppearance);
}
