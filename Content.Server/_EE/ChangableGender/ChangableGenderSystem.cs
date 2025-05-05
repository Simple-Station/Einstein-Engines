using Content.Shared.Humanoid;
using Content.Shared.Popups;
using Content.Shared.Verbs;

namespace Content.Server._EE.ChangableGender;

public sealed class ChangableGenderSystem : EntitySystem
{
    [Dependency] private readonly SharedHumanoidAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangableGenderComponent, GetVerbsEvent<AlternativeVerb>>(AddGenderChangeVerb);
    }

    private void AddGenderChangeVerb(EntityUid uid, ChangableGenderComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        if (!TryComp<ChangableGenderComponent>(uid, out var comp) || !TryComp<HumanoidAppearanceComponent>(uid, out var app))
            return;

        var priority = 0;
        foreach (var entry in component.GenderList)
        {
            AlternativeVerb selection = new()
            {
                Text = entry.Key,
                Category = VerbCategory.GenderChange,
                Priority = priority,
                Act = () =>
                {
                    _appearance.SetGender(uid, entry.Value, app);
                    _popup.PopupEntity(Loc.GetString("changable-gender-component-gender-set", ("pronouns", entry.Key)),
                        args.User, args.User);
                    if (comp.SingleUse)
                        RemCompDeferred<ChangableGenderComponent>(uid);
                }
            };

            priority--;
            args.Verbs.Add(selection);
        }
    }
}
