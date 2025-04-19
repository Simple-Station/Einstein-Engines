namespace Content.Shared.Humanoid.Markings;

/// <summary>
///     Colors layer in a skin color
/// </summary>
public sealed partial class SkinColoring : LayerColoringType
{
    public override Color? GetCleanColor(Color? skin, MarkingSet markingSet)
    {
        return skin;
    }
}
