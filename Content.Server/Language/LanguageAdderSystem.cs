using System.Linq;
using Content.Shared.Language;
using Content.Shared.Language.Components;
using Content.Shared.Language.Events;
using Content.Shared.Language.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

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
    }

}
