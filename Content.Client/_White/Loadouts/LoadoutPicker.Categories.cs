using System.Linq;
using Content.Shared.Clothing.Loadouts.Prototypes;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;


namespace Content.Client._White.Loadouts;


public sealed partial class LoadoutPicker
{
    private List<LoadoutCategoryPrototype> _rootCategories = [];

    public IEnumerable<LoadoutCategoryPrototype> GetRootCategories()
    {
        return _rootCategories;
    }

    private ILoadoutMenuEntry? _currentEntry;

    public ILoadoutMenuEntry CurrentEntry
    {
        get => _currentEntry ?? throw new InvalidOperationException();
        set
        {
            _currentEntry?.Exit(Loadouts, this);
            ClearupEdit();
            ClearLoadoutCategoryButtons();
            _currentEntry = value;
            EntryBackButton.Visible = _currentEntry.Parent != null;
            _currentEntry.Act(Loadouts, this);
        }
    }


    private void CacheRootCategories()
    {
        _rootCategories =
            _prototypeManager.EnumeratePrototypes<LoadoutCategoryPrototype>().Where(p => p.Root)
                .ToList();
    }

    private void InitializeCategories()
    {
        EntryBackButton.OnPressed += EntryBackButtonPressed;
        var rootEntry = new LoadoutEntriesContainerMenuEntry("root");
        foreach (var category in GetRootCategories())
        {
            rootEntry.AddChild(BuildMenuGroup(category.ID).Item1);
        }

        CurrentEntry = rootEntry;
    }

    private void EntryBackButtonPressed(BaseButton.ButtonEventArgs obj)
    {
        if(CurrentEntry.Parent != null)
            CurrentEntry = CurrentEntry.Parent;
    }

    private (ILoadoutMenuEntry, int) BuildMenuGroup(ProtoId<LoadoutCategoryPrototype> categoryPrototypeId)
    {
        var weight = 0;

        if(!_prototypeManager.TryIndex(categoryPrototypeId, out var categoryPrototype))
            throw new Exception($"Cannot load prototype {categoryPrototypeId}");

        if (categoryPrototype.SubCategories.Count == 0)
            return (new LoadoutCategoryShowMenuEntry(categoryPrototypeId), 1);

        var entry = new LoadoutEntriesContainerMenuEntry(categoryPrototypeId);

        foreach (var category in categoryPrototype.SubCategories)
        {
            var child = BuildMenuGroup(category);
            if(child.Item2 == 0) continue;
            entry.AddChild(child.Item1);
            weight+= child.Item2;
        }

        return (entry, weight);
    }
}
