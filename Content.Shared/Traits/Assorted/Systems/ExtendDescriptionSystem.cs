using Content.Shared.Examine;
using Content.Shared.Traits.Assorted.Components;

namespace Content.Shared.Traits.Assorted.Systems;

public sealed class ExtendDescriptionSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ExtendDescriptionComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(EntityUid uid, ExtendDescriptionComponent component, ExaminedEvent args)
    {
        if (component.DescriptionList.Count <= 0)
            return;

        foreach (var desc in component.DescriptionList)
        {
            if (!args.IsInDetailsRange && desc.RequireDetailRange)
                continue;

            args.PushMarkup($"[font size ={desc.FontSize}][color={desc.Color}]{Loc.GetString(desc.Description, ("entity", uid))}[/color][/font]");
        }
    }
}
