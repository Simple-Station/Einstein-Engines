using Content.Shared.Examine;
using Content.Shared.Traits.Assorted.Components;

namespace Content.Shared.Traits.Assorted.Systems;

public sealed class CyberEyesSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CyberEyesComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(EntityUid uid, CyberEyesComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        args.PushMarkup($"[color=white]{Loc.GetString(component.CyberEyesExaminationText, ("entity", uid))}[/color]");
    }
}
