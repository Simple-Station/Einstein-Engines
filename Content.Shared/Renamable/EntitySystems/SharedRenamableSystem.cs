using Content.Shared.Database;
using Content.Shared.Renamable.Components;
using Content.Shared.Verbs;


namespace Content.Shared.Renamable.EntitySystems;

public partial class SharedRenamableSystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _uiSystem = null!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RenamableComponent, GetVerbsEvent<Verb>>(OnGetVerbs);
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
