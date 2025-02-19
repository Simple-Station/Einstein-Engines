using Content.Shared.Database;
using Content.Shared.Popups;
using Content.Shared.Renamable.Components;
using Content.Shared.Verbs;


namespace Content.Shared.Renamable.EntitySystems;

public partial class SharedRenamableSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _uiSystem = null!;
    private SharedPopupSystem? _popup;
    private MetaDataSystem? _metaData;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RenamableComponent, GetVerbsEvent<Verb>>(OnGetVerbs);
        SubscribeLocalEvent<RenamableComponent, RenameEvent>(OnRename);
        _popup = _entManager.System<SharedPopupSystem>();
        _metaData = _entManager.System<MetaDataSystem>();
    }

    private void OnRename(Entity<RenamableComponent> entity, ref RenameEvent renameEvent)
    {
        var uid = EntityManager.GetEntity(renameEvent.NetEntity);
        _metaData!.SetEntityName(uid, renameEvent.NewName);
        _popup!.PopupPredicted(Loc.GetString("comp-renamable-rename", ("newname", renameEvent.NewName)), entity, null);
        DirtyEntity(entity.Owner);
    }

    private void OnGetVerbs(Entity<RenamableComponent> entity, ref GetVerbsEvent<Verb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands == null)
            return;
        var entityUid = entity.Owner;
        var user = args.User;
        var v = new Verb
        {
            Text = Loc.GetString("verb-categories-rename"),
            DoContactInteraction = true,
            Act = () =>
            {
                _uiSystem.OpenUi(entityUid, SharedRenamableInterfaceKey.Key, user);
            }
        };
        args.Verbs.Add(v);
    }
}

public partial class RenameEvent : EntityEventArgs
{
    public NetEntity NetEntity;
    public string NewName;

    public RenameEvent(NetEntity netEntity, string newName)
    {
        NewName = newName;
    }
}
