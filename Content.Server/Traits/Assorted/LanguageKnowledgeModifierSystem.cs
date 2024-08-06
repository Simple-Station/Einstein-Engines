using System.Linq;
using Content.Server.Language;
using Content.Shared.Language.Components;

namespace Content.Server.Traits.Assorted;

public sealed class LanguageKnowledgeModifierSystem : EntitySystem
{
    [Dependency] private readonly LanguageSystem _languages = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LanguageKnowledgeModifierComponent, ComponentInit>(OnStartup);
    }

    private void OnStartup(Entity<LanguageKnowledgeModifierComponent> entity, ref ComponentInit args)
    {
        if (!TryComp<LanguageKnowledgeComponent>(entity, out var knowledge))
        {
            Log.Warning($"Entity {entity.Owner} does not have a LanguageKnowledge but has a LanguageKnowledgeModifier!");
            return;
        }

        foreach (var spokenLanguage in entity.Comp.NewSpokenLanguages)
        {
            _languages.AddLanguage(entity, spokenLanguage, true, false, knowledge);
        }

        foreach (var understoodLanguage in entity.Comp.NewUnderstoodLanguages)
        {
            _languages.AddLanguage(entity, understoodLanguage, false, true, knowledge);
        }
    }
}
