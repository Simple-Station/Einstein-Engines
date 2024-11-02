using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Body.Events;
using Robust.Shared.GameStates;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;

namespace Content.Shared.Body.Systems;
// Code shamelessly stolen from MS14.
public partial class SharedBodySystem
{
    private void InitializePartAppearances()
    {
        base.Initialize();

        SubscribeLocalEvent<BodyPartAppearanceComponent, ComponentInit>(OnPartAppearanceInit);
        SubscribeLocalEvent<BodyPartAppearanceComponent, BodyPartAddedEvent>(OnPartAddedToBody);
    }

    private static readonly Dictionary<(BodyPartType, BodyPartSymmetry), HumanoidVisualLayers> BodyPartVisualLayers
       = new Dictionary<(BodyPartType, BodyPartSymmetry), HumanoidVisualLayers>
       {
            { (BodyPartType.Head,BodyPartSymmetry.None), HumanoidVisualLayers.Head },
            { (BodyPartType.Tail,BodyPartSymmetry.None), HumanoidVisualLayers.Tail },
            { (BodyPartType.Torso,BodyPartSymmetry.None), HumanoidVisualLayers.Chest },
            { (BodyPartType.Arm,BodyPartSymmetry.Right), HumanoidVisualLayers.RArm },
            { (BodyPartType.Arm,BodyPartSymmetry.Left), HumanoidVisualLayers.LArm },
            { (BodyPartType.Hand,BodyPartSymmetry.Right), HumanoidVisualLayers.RHand },
            { (BodyPartType.Hand,BodyPartSymmetry.Left), HumanoidVisualLayers.LHand },
            { (BodyPartType.Leg,BodyPartSymmetry.Right), HumanoidVisualLayers.RLeg },
            { (BodyPartType.Leg,BodyPartSymmetry.Left), HumanoidVisualLayers.LLeg },
            { (BodyPartType.Foot,BodyPartSymmetry.Right), HumanoidVisualLayers.RLeg },
            { (BodyPartType.Foot,BodyPartSymmetry.Left), HumanoidVisualLayers.LLeg }
       };

    private void OnPartAppearanceInit(EntityUid uid, BodyPartAppearanceComponent component, ComponentInit args)
    {

        if (TryComp(uid, out BodyPartComponent? part) && part.OriginalBody != null &&
            TryComp(part.OriginalBody.Value, out HumanoidAppearanceComponent? bodyAppearance))
        {
            var customLayers = bodyAppearance.CustomBaseLayers;
            var spriteLayers = bodyAppearance.BaseLayers;
            var visualLayer = BodyPartVisualLayers[(part.PartType, part.Symmetry)];

            component.OriginalBody = part.OriginalBody.Value;

            if (customLayers.ContainsKey(visualLayer))
            {
                component.ID = customLayers[visualLayer].Id;
                component.Color = customLayers[visualLayer].Color;
            }
            else if (spriteLayers.ContainsKey(visualLayer))
            {
                component.ID = spriteLayers[visualLayer].ID;
                component.Color = bodyAppearance.SkinColor;
            }
            else
            {
                var symmetry = ((BodyPartSymmetry) part.Symmetry).ToString();
                if (symmetry == "None")
                    symmetry = "";
                component.ID = "removed" + symmetry + ((BodyPartType) part.PartType).ToString();
                component.Color = bodyAppearance.SkinColor;
            }
        }
        Dirty(uid, component);
        UpdateAppearance(uid, component);
    }

    public void OnPartAddedToBody(EntityUid uid, BodyPartAppearanceComponent component, ref BodyPartAddedEvent args)
    {
        if (TryComp(uid, out HumanoidAppearanceComponent? bodyAppearance))
        {
            var part = args.Part;
            var customLayers = bodyAppearance.CustomBaseLayers;
            var visualLayer = BodyPartVisualLayers[(part.Comp.PartType, part.Comp.Symmetry)];
            customLayers[visualLayer] = new CustomBaseLayerInfo(component.ID, customLayers[visualLayer].Color);
        }
    }

    protected abstract void UpdateAppearance(EntityUid uid, BodyPartAppearanceComponent component);
}
