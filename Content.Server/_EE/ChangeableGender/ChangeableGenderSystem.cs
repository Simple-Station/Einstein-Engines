using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.IdentityManagement.Components;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.GameObjects.Components.Localization;

namespace Content.Server._EE.ChangeableGender;

public sealed class ChangeableGenderSystem : EntitySystem
{
    [Dependency] private readonly SharedHumanoidAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangeableGenderComponent, GetVerbsEvent<AlternativeVerb>>(AddGenderChangeVerb);
    }

    private void AddGenderChangeVerb(EntityUid uid, ChangeableGenderComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        if (!TryComp<ChangeableGenderComponent>(uid, out var comp) || !TryComp<HumanoidAppearanceComponent>(uid, out var app))
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
                    _popup.PopupEntity(Loc.GetString("changeable-gender-component-gender-set", ("pronouns", entry.Key)),
                        args.User, args.User);
                    if (TryComp<GrammarComponent>(uid, out var grammar))
                    {
                        grammar.Gender = entry.Value;
                        Dirty(uid, grammar);
                    }
                    if (TryComp<IdentityComponent>(uid, out var identity))
                    {
                        var ev = new GenderChangeEvent(uid, entry.Value);
                        RaiseLocalEvent(uid, ref ev, true);
                    }
                    if (comp.SingleUse)
                        RemCompDeferred<ChangeableGenderComponent>(uid);
                }
            };

            priority--;
            args.Verbs.Add(selection);
        }
    }
}
