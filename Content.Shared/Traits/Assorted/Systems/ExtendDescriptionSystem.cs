using Content.Shared.Customization.Systems;
using Content.Shared.Examine;
using Content.Shared.Traits.Assorted.Components;

namespace Content.Shared.Traits.Assorted.Systems;

public sealed class ExtendDescriptionSystem : EntitySystem
{
    [Dependency] private readonly CharacterRequirementsSystem _characterRequirements = default!;

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
            if (!args.IsInDetailsRange && desc.RequireDetailRange
                || !TryComp(args.Examiner, out MetaDataComponent? comp) || comp.EntityPrototype == null)
                continue;

            var meetsRequirements = desc.Requirements == null || _characterRequirements.CheckRequirementsValid(desc.Requirements, args.Examiner, comp.EntityPrototype, out _);
            var description = meetsRequirements ? desc.Description : desc.RequirementsNotMetDescription;

            if(description != string.Empty)
                args.PushMarkup($"[font size ={desc.FontSize}][color={desc.Color}]{Loc.GetString(description, ("entity", uid))}[/color][/font]");
        }
    }
}
