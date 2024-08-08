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

        foreach (var (metabolizer, _) in metabolizers)
        {
            if (metabolizer.MetabolizerTypes is null
                || metabolizer.MetabolismGroups is null)
                continue;

            foreach (var metabolismGroup in metabolizer.MetabolismGroups)
            {
                // Add the DrunkenResilience metabolizer type to the liver and equivalent organs.
                if (metabolismGroup.Id == "Alcohol")
                    metabolizer.MetabolizerTypes.Add("DrunkenResilience");
            }
        }
    }
}
