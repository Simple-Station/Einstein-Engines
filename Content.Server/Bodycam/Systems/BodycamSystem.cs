using Content.Server.Bodycam.Components;
using Content.Shared.Clothing;
using Content.Shared.Hands;
using Content.Shared.Inventory.Events;
using Content.Shared.StationAi;

namespace Content.Server.Bodycam.Systems;

public sealed class BodycamSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BodycamComponent, ClothingGotEquippedEvent>(OnGotEquipped);
        SubscribeLocalEvent<BodycamComponent, ClothingGotUnequippedEvent>(OnGotUnequipped);
    }

    private void OnGotEquipped(EntityUid uid, BodycamComponent component, ref ClothingGotEquippedEvent args)
    {
        EnsureComp<StationAiVisionComponent>(args.Wearer);
    }

    private void OnGotUnequipped(EntityUid uid, BodycamComponent component, ref ClothingGotUnequippedEvent args)
    {
        RemComp<StationAiVisionComponent>(args.Wearer);
    }
}
