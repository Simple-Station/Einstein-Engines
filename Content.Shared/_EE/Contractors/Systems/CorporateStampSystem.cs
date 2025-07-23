using Content.Shared._EE.Contractors.Components;
using Content.Shared._EE.Contractors.Prototypes;
using Content.Shared.Clothing.Loadouts.Systems;
using Content.Shared.Paper;
using Content.Shared.Preferences;
using Robust.Shared.Prototypes;

namespace Content.Shared._EE.Contractors.Systems;

public sealed class CorporateStampSystem : EntitySystem
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

    public void UpdateCorporateStamp(EntityUid ent, HumanoidCharacterProfile profile) =>
        UpdateCorporateStamp(ent, profile.Employer);

    public void UpdateCorporateStamp(EntityUid ent, ProtoId<EmployerPrototype> employerId, bool command = false)
    {
        if (!_entityManager.TryGetComponent<StampComponent>(ent, out var stampComponent) || !_prototypeManager.TryIndex(employerId, out var employer))
            return;

        stampComponent.StampedName = command ? $"{employer.NameKey}_command" : employer.NameKey;
        stampComponent.StampedColor = employer.PrimaryColour;
        stampComponent.StampState = $"{StampStateBase}-{employer.ID.ToLower()}";
    }
}
