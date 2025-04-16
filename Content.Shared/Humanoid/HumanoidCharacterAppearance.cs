using System.Linq;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Store;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization;

namespace Content.Shared.Humanoid;

[DataDefinition]
[Serializable, NetSerializable]
public sealed partial class HumanoidCharacterAppearance : ICharacterAppearance, IEquatable<HumanoidCharacterAppearance>
{
    [DataField]
    public Color EyeColor { get; private set; }

    [DataField]
    public Color SkinColor { get;  set; }

    [DataField]
    public List<Marking> Markings { get; private set; } = new();

    public HumanoidCharacterAppearance(
        Color skinColor,
        List<Marking> markings)
    {
        SkinColor = ClampColor(skinColor);
        Markings = markings;
    }

    public HumanoidCharacterAppearance(HumanoidCharacterAppearance other)
        : this(
            other.SkinColor,
            new(other.Markings))
    {

    }

    public HumanoidCharacterAppearance WithSkinColor(Color newColor)
    {
        return new(newColor, Markings);
    }

    public HumanoidCharacterAppearance WithMarkings(List<Marking> newMarkings)
    {
        return new(SkinColor, newMarkings);
    }

    public static HumanoidCharacterAppearance DefaultWithSpecies(string species)
    {
        var speciesPrototype = IoCManager.Resolve<IPrototypeManager>().Index<SpeciesPrototype>(species);
        var skinColor = speciesPrototype.SkinColoration switch
        {
            HumanoidSkinColor.HumanToned => Humanoid.SkinColor.HumanSkinTone(speciesPrototype.DefaultHumanSkinTone),
            HumanoidSkinColor.Hues => speciesPrototype.DefaultSkinTone,
            HumanoidSkinColor.TintedHues => Humanoid.SkinColor.TintedHues(speciesPrototype.DefaultSkinTone),
            HumanoidSkinColor.VoxFeathers => Humanoid.SkinColor.ClosestVoxColor(speciesPrototype.DefaultSkinTone),
            HumanoidSkinColor.AnimalFur => Humanoid.SkinColor.ClosestAnimalFurColor(speciesPrototype.DefaultSkinTone), // Einstein Engines - Tajaran
            HumanoidSkinColor.TintedHuesSkin => Humanoid.SkinColor.TintedHuesSkin(speciesPrototype.DefaultSkinTone, speciesPrototype.DefaultSkinTone),
            _ => Humanoid.SkinColor.ValidHumanSkinTone,
        };

        return new(
            skinColor,
            new ()
        );
    }

    private static IReadOnlyList<Color> RealisticEyeColors = new List<Color>
    {
        Color.Brown,
        Color.Gray,
        Color.Azure,
        Color.SteelBlue,
        Color.Black
    };

    public static HumanoidCharacterAppearance Random(string species, Sex sex)
    {
        var random = IoCManager.Resolve<IRobustRandom>();
        var markingManager = IoCManager.Resolve<MarkingManager>();
        var hairStyles = markingManager.MarkingsByCategoryAndSpecies(MarkingCategories.Hair, species).Keys.ToList();
        var facialHairStyles = markingManager.MarkingsByCategoryAndSpecies(MarkingCategories.FacialHair, species).Keys.ToList();

        var newHairStyle = hairStyles.Count > 0
            ? random.Pick(hairStyles)
            : HairStyles.DefaultHairStyle;

        var newFacialHairStyle = facialHairStyles.Count == 0 || sex == Sex.Female
            ? HairStyles.DefaultFacialHairStyle
            : random.Pick(facialHairStyles);

        var newHairColor = random.Pick(HairStyles.RealisticHairColors);
        newHairColor = newHairColor
            .WithRed(RandomizeColor(newHairColor.R))
            .WithGreen(RandomizeColor(newHairColor.G))
            .WithBlue(RandomizeColor(newHairColor.B));

        var newHairMarking = new Marking(newHairStyle, new List<Color> { newHairColor });
        var newFacialHairMarking = new Marking(newFacialHairStyle, new List<Color> { newHairColor });

        // TODO: Add random markings

        var skinType = IoCManager.Resolve<IPrototypeManager>().Index<SpeciesPrototype>(species).SkinColoration;
            var skinTone = IoCManager.Resolve<IPrototypeManager>().Index<SpeciesPrototype>(species).DefaultSkinTone; // DeltaV, required for tone blending

        var newSkinColor = new Color(random.NextFloat(1), random.NextFloat(1), random.NextFloat(1), 1);
        switch (skinType)
        {
            case HumanoidSkinColor.HumanToned:
                var tone = Math.Round(Humanoid.SkinColor.HumanSkinToneFromColor(newSkinColor));
                newSkinColor = Humanoid.SkinColor.HumanSkinTone((int)tone);
                break;
            case HumanoidSkinColor.Hues:
                break;
            case HumanoidSkinColor.TintedHues:
                newSkinColor = Humanoid.SkinColor.ValidTintedHuesSkinTone(newSkinColor);
                    break;
            case HumanoidSkinColor.TintedHuesSkin:
                newSkinColor = Humanoid.SkinColor.ValidTintedHuesSkinTone(skinTone, newSkinColor);
                break;
            case HumanoidSkinColor.VoxFeathers:
                newSkinColor = Humanoid.SkinColor.ProportionalVoxColor(newSkinColor);
                break;
            case HumanoidSkinColor.AnimalFur: // Einstein Engines - Tajaran
                newSkinColor = Humanoid.SkinColor.ProportionalAnimalFurColor(newSkinColor);
                break;
        }

        return new HumanoidCharacterAppearance(newSkinColor, new List<Marking> { newHairMarking, newFacialHairMarking });

        float RandomizeColor(float channel)
        {
            return MathHelper.Clamp01(channel + random.Next(-25, 25) / 100f);
        }
    }

    public static Color ClampColor(Color color)
    {
        return new(color.RByte, color.GByte, color.BByte);
    }

    public static HumanoidCharacterAppearance EnsureValid(HumanoidCharacterAppearance appearance, string species, Sex sex)
    {
        var eyeColor = ClampColor(appearance.EyeColor);

        var proto = IoCManager.Resolve<IPrototypeManager>();
        var markingManager = IoCManager.Resolve<MarkingManager>();

        var markingSet = new MarkingSet();
        var skinColor = appearance.SkinColor;
        if (proto.TryIndex(species, out SpeciesPrototype? speciesProto))
        {
            markingSet = new MarkingSet(appearance.Markings, speciesProto.MarkingPoints, markingManager, proto);
            markingSet.EnsureValid(markingManager);

            if (!Humanoid.SkinColor.VerifySkinColor(speciesProto.SkinColoration, skinColor))
            {
                skinColor = Humanoid.SkinColor.ValidSkinTone(speciesProto.SkinColoration, skinColor);
            }

            markingSet.EnsureSpecies(species, skinColor, markingManager);
            markingSet.EnsureSexes(sex, markingManager);
        }

        return new HumanoidCharacterAppearance(
            skinColor,
            markingSet.GetForwardEnumerator().ToList());
    }

    public bool MemberwiseEquals(ICharacterAppearance maybeOther)
    {
        return
            maybeOther is HumanoidCharacterAppearance other
            && EyeColor.Equals(other.EyeColor)
            && SkinColor.Equals(other.SkinColor)
            && Markings.SequenceEqual(other.Markings);
    }

    public bool Equals(HumanoidCharacterAppearance? other)
    {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return EyeColor.Equals(other.EyeColor)
            && SkinColor.Equals(other.SkinColor)
            && Markings.SequenceEqual(other.Markings);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj)
            || obj is HumanoidCharacterAppearance other
                && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            EyeColor,
            SkinColor,
            Markings);
    }

    public HumanoidCharacterAppearance Clone() => new(this);
}
