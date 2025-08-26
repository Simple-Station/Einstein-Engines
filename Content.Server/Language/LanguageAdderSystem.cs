using System.Linq;
using Content.Shared.Language;
using Content.Shared.Language.Components;
using Content.Shared.Language.Events;
using Content.Shared.Language.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server.Language;

public sealed class LanguageAdderSystem : EntitySystem
{

    [Dependency] private readonly LanguageSystem _languageSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LanguageAdderComponent, MapInitEvent>(OnInitLanguageAdder);
    }

    private void OnInitLanguageAdder(Entity<LanguageAdderComponent> ent, ref MapInitEvent args) //might need to move this to componentInit
    {
        if (ent.Comp.AddedSpokenLanguages is not null)
            foreach (var lang in ent.Comp.AddedSpokenLanguages)
                _languageSystem.AddLanguage(ent.Owner, lang, true, false);

        if (ent.Comp.AddedUnderstoodLanguages is not null)
            foreach (var lang in ent.Comp.AddedUnderstoodLanguages)
                _languageSystem.AddLanguage(ent.Owner, lang, false, true);

        // making the language be selected first, so that newfriends aren't signing instead of speaking

        if (!TryComp<LanguageSpeakerComponent>(ent.Owner, out var comp)) //this fetches language speaker comp, but should never ever fail
            return;

        comp.CurrentLanguage = comp.SpokenLanguages.First();
        RaiseLocalEvent(ent, new LanguagesUpdateEvent());
        Dirty(ent);

    }

}
