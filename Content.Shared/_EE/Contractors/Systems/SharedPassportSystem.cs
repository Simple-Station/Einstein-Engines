using Content.Shared._EE.Contractors.Components;
using Content.Shared._EE.Contractors.Prototypes;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Examine;
using Content.Shared.GameTicking;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory;
using Content.Shared.Item;
using Content.Shared.Preferences;
using Content.Shared.Storage;
using Content.Shared.Storage.EntitySystems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;


namespace Content.Shared._EE.Contractors.Systems;

public class SharedPassportSystem : EntitySystem
{
    public const int CurrentYear = 2465;
    const string PIDChars = "ABCDEFGHJKLMNPQRSTUVWXYZ0123456789";

    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedStorageSystem _storage = default!;
    [Dependency] private readonly SharedTransformSystem _sharedTransformSystem = default!;
    [Dependency] private readonly SharedAdminLogSystem _log = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PassportComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
        SubscribeLocalEvent<PassportComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(EntityUid uid, PassportComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange && !component.IsClosed)
        {
            args.PushMarkup($"Registered to: {component.OwnerProfile.Name}");
            args.PushMarkup($"Species: {component.OwnerProfile.Species}");
            args.PushMarkup($"Sex: {component.OwnerProfile.Gender}");
            args.PushMarkup($"Height: {component.OwnerProfile.Height}");
            args.PushMarkup($"Year of Birth: {CurrentYear - component.OwnerProfile.Age}");
            args.PushMarkup($"PID: {GenerateIdentityString(component.OwnerProfile.Name + component.OwnerProfile.Height + component.OwnerProfile.Age + component.OwnerProfile.Height + component.OwnerProfile.FlavorText)}");
        }
    }

    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent ev)
    {
        if (Deleted(ev.Mob) || !Exists(ev.Mob))
            return;

        if (!_prototypeManager.TryIndex(
            ev.Profile.Nationality,
            out NationalityPrototype? nationalityPrototype) || !_prototypeManager.TryIndex(nationalityPrototype.PassportPrototype, out EntityPrototype? entityPrototype))
            return;

        var passportEntity = _entityManager.SpawnEntity(entityPrototype.Name, _sharedTransformSystem.GetMapCoordinates(ev.Mob));
        var passportComponent = _entityManager.GetComponent<PassportComponent>(passportEntity);

        UpdatePassportProfile(new(passportEntity, passportComponent), ev.Profile);

        // Try to find back-mounted storage apparatus
        if (_inventory.TryGetSlotEntity(ev.Mob, "back", out var item) &&
                EntityManager.TryGetComponent<StorageComponent>(item, out var inventory))
            // Try inserting the entity into the storage, if it can't, it leaves the loadout item on the ground
        {
            if (!EntityManager.TryGetComponent<ItemComponent>(passportEntity, out var itemComp)
                || !_storage.CanInsert(item.Value, passportEntity, out _, inventory, itemComp)
                || !_storage.Insert(item.Value, passportEntity, out _, playSound: false))
            {
                _log.Add(
                    LogType.EntitySpawn,
                    LogImpact.Low,
                    $"Passport for {ev.Profile.Name} was spawned on the floor due to missing bag space");
            }
        }
    }

    public void UpdatePassportProfile(Entity<PassportComponent> passport, HumanoidCharacterProfile profile)
    {
        passport.Comp.OwnerProfile = profile;
        var evt = new PassportProfileUpdatedEvent(profile);
        RaiseLocalEvent(passport, ref evt);
    }

    private void OnUseInHand(Entity<PassportComponent> passport, ref UseInHandEvent evt)
    {
        if (evt.Handled || !_timing.IsFirstTimePredicted)
            return;

        evt.Handled = true;
        passport.Comp.IsClosed = !passport.Comp.IsClosed;

        var passportEvent = new PassportToggleEvent();
        RaiseLocalEvent(passport, ref passportEvent);
    }

    private static string GenerateIdentityString(string seed)
    {
        var hashCode = seed.GetHashCode();
        System.Random random = new System.Random(hashCode);

        char[] result = new char[17]; // 15 characters + 2 dashes

        int j = 0;
        for (int i = 0; i < 15; i++)
        {
            if (i == 5 || i == 10)
            {
                result[j++] = '-';
            }
            result[j++] = PIDChars[random.Next(PIDChars.Length)];
        }

        return new string(result);
    }

    [ByRefEvent]
    public sealed class PassportToggleEvent : HandledEntityEventArgs {}

    [ByRefEvent]
    public sealed class PassportProfileUpdatedEvent(HumanoidCharacterProfile profile) : HandledEntityEventArgs
    {
        public HumanoidCharacterProfile Profile { get; } = profile;
    }
}
