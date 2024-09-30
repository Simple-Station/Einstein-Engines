using Robust.Shared.Serialization;

namespace Content.Shared.ListViewSelector;

[Serializable, NetSerializable]
public record ListViewSelectorEntry(string Id, string Name = "", string Description = "");

[Serializable, NetSerializable]
public enum ListViewSelectorUiKey
{
    Key
}

[Serializable, NetSerializable]
public sealed class ListViewSelectorState(List<ListViewSelectorEntry> items) : BoundUserInterfaceState
{
    public List<ListViewSelectorEntry> Items { get; } = items;
}

[Serializable, NetSerializable]
public sealed class ListViewItemSelectedMessage(ListViewSelectorEntry selectedItem, int index)
    : BoundUserInterfaceMessage
{
    public ListViewSelectorEntry SelectedItem { get; private set; } = selectedItem;
    public int Index { get; private set; } = index;
}
