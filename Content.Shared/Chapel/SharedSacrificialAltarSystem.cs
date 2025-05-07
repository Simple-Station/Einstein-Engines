using System.Linq;
using Content.Shared.Buckle.Components;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Verbs;

namespace Content.Shared.Chapel;

public abstract partial class SharedSacrificialAltarSystem : EntitySystem
{
    [Dependency] protected readonly SharedDoAfterSystem DoAfter = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SacrificialAltarComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<SacrificialAltarComponent, UnbuckledEvent>(OnUnstrapped);
        SubscribeLocalEvent<SacrificialAltarComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
    }

    private void OnExamined(Entity<SacrificialAltarComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("altar-examine"));
    }

    private void OnUnstrapped(Entity<SacrificialAltarComponent> ent, ref UnbuckledEvent args)
    {
        if (ent.Comp.DoAfter is not { } id)
            return;

        DoAfter.Cancel(id);
        ent.Comp.DoAfter = null;
    }

    private void OnGetVerbs(Entity<SacrificialAltarComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || ent.Comp.DoAfter != null
            || !TryComp<StrapComponent>(ent, out var strap)
            || GetFirstBuckled(strap) is not { } target)
            return;

        var user = args.User;
        args.Verbs.Add(new AlternativeVerb()
        {
            Act = () => AttemptSacrifice(ent, user, target),
            Text = Loc.GetString("altar-sacrifice-verb"),
            Priority = 2
        });
    }

    private EntityUid? GetFirstBuckled(StrapComponent strap)
    {
        if (strap.BuckledEntities.Count <= 0)
            return null;

        return strap.BuckledEntities.First();
    }

    protected virtual void AttemptSacrifice(Entity<SacrificialAltarComponent> ent, EntityUid user, EntityUid target) { }
}
