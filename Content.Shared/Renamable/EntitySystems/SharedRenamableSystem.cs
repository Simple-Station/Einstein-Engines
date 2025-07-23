using System.Diagnostics;
using Content.Shared.Database;
using Content.Shared.NameIdentifier;
using Content.Shared.Popups;
using Content.Shared.Renamable.Components;
using Content.Shared.Verbs;
using Robust.Shared.Serialization;


namespace Content.Shared.Renamable.EntitySystems;

public partial class SharedRenamableSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _uiSystem = null!;
    private SharedPopupSystem? _popup;
    private MetaDataSystem _metaData = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RenamableComponent, GetVerbsEvent<Verb>>(OnGetVerbs);
        SubscribeLocalEvent<RenamableComponent, RenamableBuiMessage>(OnRename);

        _popup = _entManager.System<SharedPopupSystem>();
        _metaData = _entManager.System<MetaDataSystem>();
    }

    private void OnRename(Entity<RenamableComponent> entity, ref RenamableBuiMessage renameMessage)
    {
        _popup!.PopupPredicted(Loc.GetString("comp-renamable-rename", ("newname", renameMessage.Name)), entity, null);

        var name = renameMessage.Name.Trim();

        var metaData = MetaData(entity);

        // don't change the name if the value doesn't actually change
        if (metaData.EntityName.Equals(name, StringComparison.InvariantCulture))
            return;

        if (TryComp<NameIdentifierComponent>(entity, out var identifier))
            name = $"{name} {identifier.FullIdentifier}";

        _metaData.SetEntityName(entity, name, metaData);
        if (entity.Comp.SingleUse)
            RemCompDeferred<RenamableComponent>(entity);
    }

    private void OnGetVerbs(Entity<RenamableComponent> entity, ref GetVerbsEvent<Verb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands == null)
            return;

        var entityUid = entity.Owner;
        var user = args.User;
        var renameVerb = new Verb
        {
            Text = Loc.GetString("verb-categories-rename"),
            DoContactInteraction = true,
            Act = () =>
            {
                _uiSystem.OpenUi(entityUid, SharedRenamableInterfaceKey.Key, user);
            }
        };
        args.Verbs.Add(renameVerb);
    }
}

[Serializable, NetSerializable]
public sealed class RenamableBuiMessage(string name) : BoundUserInterfaceMessage
{
    public string Name = name;
}
