namespace Content.Shared.Traits.Assorted.Components;

public sealed partial class DescriptionExtension
{
    [DataField]
    public string Description = "";

    [DataField]
    public int FontSize = 12;

    [DataField]
    public Color Color = Color.White;

    [DataField]
    public bool RequireDetailRange = true;
}

[RegisterComponent]
public sealed partial class ExtendDescriptionComponent : Component
{
    /// <summary>
    ///     The list of all descriptions to add to an entity when examined at close range.
    /// </summary>
    [DataField]
    public List<DescriptionExtension> DescriptionList = new();
}
