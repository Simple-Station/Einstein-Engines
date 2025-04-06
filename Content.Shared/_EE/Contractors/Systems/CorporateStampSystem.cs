using Content.Shared._EE.Contractors.Components;
using Content.Shared._EE.Contractors.Prototypes;
using Content.Shared.Clothing.Loadouts.Systems;
using Content.Shared.Paper;
using Content.Shared.Preferences;
using Robust.Shared.Prototypes;

namespace Content.Shared._EE.Contractors.Systems;

public class CorporateStampSystem : EntitySystem
{
    private const string StampStateBase = "paper_stamp-corporate";

    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CorporateStampComponent, SpawnedViaLoadoutEvent>(OnSpawnedViaLoadout);
    }

    private void OnSpawnedViaLoadout(Entity<CorporateStampComponent> ent, ref SpawnedViaLoadoutEvent args) =>
        UpdateCorporateStamp(ent, args.Profile);

    public void UpdateCorporateStamp(EntityUid ent, HumanoidCharacterProfile profile)
    {
        if (!_entityManager.TryGetComponent<StampComponent>(ent, out var stampComponent) || !_prototypeManager.TryIndex<EmployerPrototype>(profile.Employer, out var employer))
            return;

        stampComponent.StampedName = employer.NameKey;
        stampComponent.StampedColor = employer.PrimaryColour;
        stampComponent.StampState = $"{StampStateBase}-{employer.ID.ToLower()}";
    }
}
