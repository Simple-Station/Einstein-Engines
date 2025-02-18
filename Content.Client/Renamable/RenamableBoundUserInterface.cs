using Content.Shared.NameModifier.EntitySystems;
using Content.Shared.Popups;
using Robust.Client.UserInterface;

namespace Content.Client.Renamable;

public sealed class RenamableBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    private readonly MetaDataSystem _metaData;
    private readonly SharedPopupSystem _popup;
    private readonly NameModifierSystem _nameModifier;
    private EntityQuery<MetaDataComponent> _metaQuery;

    [ViewVariables]
    private RenamableWindow? _window;

    public RenamableBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        _metaQuery = _entManager.GetEntityQuery<MetaDataComponent>();
        _metaData = _entManager.System<MetaDataSystem>();
        _popup = _entManager.System<SharedPopupSystem>();
        _nameModifier = _entManager.System<NameModifierSystem>();
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

        _window.SetCurrentName(metadata.EntityName);
    }
}
