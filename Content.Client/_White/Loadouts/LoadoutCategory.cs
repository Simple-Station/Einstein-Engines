using System.Numerics;
using Content.Shared.Clothing.Loadouts.Prototypes;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;


namespace Content.Client._White.Loadouts;


public interface ILoadoutMenuEntry
{
    public ILoadoutMenuEntry? Parent { get; set; }
    public string Label { get; }

    public void Act(BoxContainer loadoutsContainer, LoadoutPicker loadoutPicker);
    public void Exit(BoxContainer loadoutsContainer , LoadoutPicker loadoutPicker);
}

public sealed class LoadoutCategoryShowMenuEntry : ILoadoutMenuEntry
{
    private readonly ProtoId<LoadoutCategoryPrototype> _loadoutCategory;
    public ILoadoutMenuEntry? Parent { get; set; }
    public string Label { get; }

    public LoadoutCategoryShowMenuEntry(ProtoId<LoadoutCategoryPrototype> loadoutCategory)
    {
        _loadoutCategory = loadoutCategory;
        Label = Loc.GetString($"loadout-category-{loadoutCategory}");
    }

    public void Act(BoxContainer loadoutsContainer, LoadoutPicker loadoutPicker)
    {
        loadoutPicker.LoadCategoryButtons(_loadoutCategory);
    }

    public void Exit(BoxContainer loadoutsContainer, LoadoutPicker loadoutPicker)
    {

    }
}

public sealed class LoadoutEntriesContainerMenuEntry : ILoadoutMenuEntry
{
    public ILoadoutMenuEntry? Parent { get; set;}
    public string Label { get; }

    private readonly List<ILoadoutMenuEntry> _children = [];
    public IReadOnlyList<ILoadoutMenuEntry> Children => _children;
    private readonly List<(BaseButton, Action<BaseButton.ButtonEventArgs>)> _currBrns = [];

    public LoadoutEntriesContainerMenuEntry(ProtoId<LoadoutCategoryPrototype> loadoutCategoryProtoId)
    {
        Label = Loc.GetString($"loadout-category-{loadoutCategoryProtoId}");
    }

    public LoadoutEntriesContainerMenuEntry(string label)
    {
        Label = label;
    }

    public void AddChild(params ILoadoutMenuEntry[] children)
    {
        foreach (var child in children)
        {
            _children.Add(child);
            child.Parent = this;
        }
    }

    public void Act(BoxContainer loadoutsContainer, LoadoutPicker loadoutPicker)
    {
        foreach (var menuEntry in _children)
        {
            var button = new Button()
            {
                Children =
                {
                    new BoxContainer()
                    {
                        Orientation = BoxContainer.LayoutOrientation.Horizontal,
                        SeparationOverride = 15,
                        Children =
                        {
                            new Label()
                            {
                                Text = menuEntry.Label,
                            }
                        }
                    }
                }
            };

            Action<BaseButton.ButtonEventArgs> handler = (_) => loadoutPicker.CurrentEntry = menuEntry;
            button.OnPressed += handler;

            _currBrns.Add((button, handler));
            loadoutsContainer.AddChild(button);
        }
    }

    public void Exit(BoxContainer loadoutsContainer, LoadoutPicker loadoutPicker)
    {
        foreach (var (button, handler) in _currBrns)
        {
            button.OnPressed -= handler;
        }
        _currBrns.Clear();
    }
}
