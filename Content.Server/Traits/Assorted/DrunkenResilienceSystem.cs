using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Tag;

namespace Content.Server.Traits.Assorted;

// TODO: make Drunken Resilience healing scale by the amount of alcohol/drunkenness
// perhaps by using `public override void Update(float frameTime)`
public sealed class DrunkenResilienceSystem : EntitySystem
{
    [Dependency] private readonly BodySystem _bodySystem = default!;
    [Dependency] private readonly TagSystem _tagSystem = default!;

    private readonly HashSet<String> MetabolizerGroups = new HashSet<String>() {
        // Stomach
        "Food",
        "Drink",
        // Heart
        "Medicine",
        "Poison",
        "Narcotic",
        // Liver
        "Alcohol",
    };

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DrunkenResilienceComponent, ComponentInit>(OnSpawn);
    }

    private void OnSpawn(Entity<DrunkenResilienceComponent> entity, ref ComponentInit args)
    {
        if (!TryComp<BodyComponent>(entity, out var body))
            return;

        if (!_bodySystem.TryGetBodyOrganComponents<MetabolizerComponent>(entity, out var metabolizers, body))
            return;

        // Add the DrunkenResilience metabolizer type to Heart/Stomach/Liver and equivalents.
        foreach (var (metabolizer, _) in metabolizers)
        {
            if (metabolizer.MetabolizerTypes is null
                || metabolizer.MetabolismGroups is null)
                continue;

            foreach (var metabolismGroup in metabolizer.MetabolismGroups)
            {
                // Kinda hacky way to check if the organ is a Heart, Stomach or Liver,
                // or anything that would process ethanol.
                if (MetabolizerGroups.Contains(metabolismGroup.Id))
                {
                    metabolizer.MetabolizerTypes.Add("DrunkenResilience");
                    continue;
                }
            }
        }
    }
}
