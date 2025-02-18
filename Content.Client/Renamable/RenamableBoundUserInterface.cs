using Content.Shared.Labels;
using Content.Shared.Labels.Components;
using Content.Shared.NameModifier.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Renamable.Components;
using Robust.Client.UserInterface;


namespace Content.Client.Renamable;


public sealed class RenamableBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly NameModifierSystem _nameModifier = default!;
    [Dependency] private EntityQuery<MetaDataComponent> _metaQuery = default!;

    [ViewVariables]
    private RenamableWindow? _window;

    public RenamableBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<RenamableWindow>();

        _window.OnNameChanged += OnNameChanged;
        Reload();
    }

    private void OnNameChanged(string newName)
    {
        _popup.PopupPredicted(Loc.GetString("comp-renamable-rename", ("newname", newName)), Owner, null);
        _metaData.SetEntityName(Owner, newName);
        _nameModifier.RefreshNameModifiers(Owner);
    }

    private void Reload()
    {
        if (_window == null)
            return;

        MetaDataComponent? metadata = null;
        if (!_metaQuery.Resolve(Owner, ref metadata))
            return;

        _window.SetCurrentLabel(metadata.EntityName);
    }
}
