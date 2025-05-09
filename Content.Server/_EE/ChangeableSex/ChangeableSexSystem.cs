using Content.Shared.Humanoid;
using Content.Shared.Popups;
using Content.Shared.Verbs;

namespace Content.Server._EE.ChangeableSex;

public sealed class ChangeableSexSystem : EntitySystem
{
    [Dependency] private readonly SharedHumanoidAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangeableSexComponent, GetVerbsEvent<AlternativeVerb>>(AddSexChangeVerb);
    }

    private void AddSexChangeVerb(EntityUid uid, ChangeableSexComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        if (!TryComp<ChangeableSexComponent>(uid, out var comp) || !TryComp<HumanoidAppearanceComponent>(uid, out var app))
            return;

        var priority = 0;
        foreach (var entry in component.SexList)
        {
            AlternativeVerb selection = new()
            {
                Text = entry.Key,
                Category = VerbCategory.SexChange,
                Priority = priority,
                Act = () =>
                {
                    _appearance.SetSex(uid, entry.Value, true, app);
                    _popup.PopupEntity(Loc.GetString("changeable-sex-component-sex-set", ("sex", entry.Key)),
                        args.User, args.User);
                    if (comp.SingleUse)
                        RemCompDeferred<ChangeableSexComponent>(uid);
                }
            };

            priority--;
            args.Verbs.Add(selection);
        }
    }
}
