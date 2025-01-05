using Content.Shared.Clothing.Components;
using Content.Shared.Inventory.Events;
using Content.Shared.NPC.Components;
using Content.Shared.Nyanotrasen.NPC.Components.Faction;
using Content.Shared.Store;

namespace Content.Shared.NPC.Systems;

public sealed partial class NpcFactionSystem
{

    public void InitializeItems()
    {
        SubscribeLocalEvent<NpcFactionMemberComponent, StoreBuyFinishedEvent>(OnStoreBuyFinished);

        SubscribeLocalEvent<ClothingAddFactionComponent, GotEquippedEvent>(OnClothingEquipped);
        SubscribeLocalEvent<ClothingAddFactionComponent, GotUnequippedEvent>(OnClothingUnequipped);
    }

    /// <summary>
    /// If we bought something we probably don't want it to start biting us after it's automatically placed in our hands.
    /// If you do, consider finding a better solution to grenade penguin CBT.
    /// </summary>
    // TODO: check if this is the right uid?? we might be cooked.
    private void OnStoreBuyFinished(
        Entity<NpcFactionMemberComponent> entity,
        ref StoreBuyFinishedEvent args
    ) =>
        entity.Comp.ExceptionalFriendlies.Add(args.Buyer);

    private void OnClothingEquipped(EntityUid uid, ClothingAddFactionComponent component, GotEquippedEvent args)
    {
        if (!TryComp<ClothingComponent>(uid, out var clothing))
            return;

        if (!clothing.Slots.HasFlag(args.SlotFlags))
            return;

        if (!TryComp<NpcFactionMemberComponent>(args.Equipee, out var factionComponent))
            return;

        if (factionComponent.Factions.Contains(component.Faction))
            return;

        component.IsActive = true;
        AddFaction(args.Equipee, component.Faction);
    }

    private void OnClothingUnequipped(EntityUid uid, ClothingAddFactionComponent component, GotUnequippedEvent args)
    {
        if (!component.IsActive)
            return;

        component.IsActive = false;
        RemoveFaction(args.Equipee, component.Faction);
    }
}
