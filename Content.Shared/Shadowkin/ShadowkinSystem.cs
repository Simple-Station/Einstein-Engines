using Content.Shared.Examine;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Humanoid;
using Content.Shared.Psionics;

namespace Content.Shared.Shadowkin;
public sealed class ShadowkinSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShadowkinComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<ShadowkinComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<ShadowkinComponent, OnMindbreakEvent>(OnMindbreak);
    }

    private void OnInit(EntityUid uid, ShadowkinComponent component, MapInitEvent args)
    {
        if (component.BlackeyeSpawn
        && TryComp<PsionicComponent>(uid, out var magic))
        {
            magic.Removable = true;
            RaiseLocalEvent(uid, new MindbreakEvent(uid), false);
        }
    }

    private void OnExamined(EntityUid uid, ShadowkinComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange
        || !TryComp<PsionicComponent>(uid, out var magic)
        || HasComp<MindbrokenComponent>(uid))
            return;

        // TODO Fetch PowerType.
        var powerType = "";

        if (args.Examined == args.Examiner)
        {
            // TODO power, powerMax need to be the value of Psionic Mana
            args.PushMarkup(Loc.GetString("shadowkin-power-examined-self",
                ("power", "0"),
                ("powerMax", "100"),
                ("powerType", powerType)
            ));
        }
        else
        {
            // TODO power, powerMax need to be the value of Psionic Mana
            args.PushMarkup(Loc.GetString("shadowkin-power-examined-other",
                ("target", uid),
                ("powerType", powerType)
            ));
        }
    }

    private void OnMindbreak(EntityUid uid, ShadowkinComponent component, ref OnMindbreakEvent args)
    {
        if (TryComp<MindbrokenComponent>(uid, out var mindbreak))
            mindbreak.MindbrokenExaminationText = "examine-mindbroken-shadowkin-message";

        if (TryComp<HumanoidAppearanceComponent>(uid, out var humanoid))
        {
            component.OldEyeColor = humanoid.EyeColor;
            humanoid.EyeColor = component.BlackEyeColor;
        }
    }
}