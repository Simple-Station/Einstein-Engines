using Content.Shared.Database;
using Content.Shared.Renamable.Components;
using Content.Shared.Verbs;


namespace Content.Shared.Renamable.EntitySystems;

public abstract class SharedRenamableSystem : EntitySystem
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

        var v = new Verb
        {
            Priority = 1,
            Category = VerbCategory.Rename,
            Text = "verb-categories-rename",
            Impact = LogImpact.Low,
            DoContactInteraction = true,
            Act = () =>
            {
                _uiSystem.OpenUi(entity.Owner, SharedRenamableInterfaceKey.Key, entity.Owner);
            }
        };
        args.Verbs.Add(v);
    }
}
