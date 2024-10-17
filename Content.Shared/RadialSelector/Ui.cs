using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.RadialSelector;

[NetSerializable, Serializable]
public enum RadialSelectorUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class RadialSelectorState(List<RadialSelectorEntry> entries, bool openCentered = false)
    : BoundUserInterfaceState
{
    [DataField(required: true)]
    public List<RadialSelectorEntry> Entries = entries;

    public bool OpenCentered { get; } = openCentered;
}

[Serializable, NetSerializable]
public sealed class RadialSelectorSelectedMessage(string selectedItem) : BoundUserInterfaceMessage
{
    public string SelectedItem { get; private set; } = selectedItem;
}

[DataDefinition]
public sealed partial class RadialSelectorEntry
{
    [DataField]
    public string? Prototype { get; set; }

    [DataField]
    public RadialSelectorCategory? Category { get; set; }
}

[DataDefinition]
public sealed partial class RadialSelectorCategory
{
    [DataField]
    public string Name { get; set; } = string.Empty;

    [DataField]
    public SpriteSpecifier Icon { get; set; } = default!;

    [DataField]
    public List<RadialSelectorEntry> Entries { get; set; } = new();
}
