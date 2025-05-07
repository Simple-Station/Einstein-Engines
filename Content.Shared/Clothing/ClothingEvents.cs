
using Content.Shared.Actions;
using Content.Shared.Clothing.Components;

namespace Content.Shared.Clothing;

/// Raised directed at a piece of clothing to get the set of layers to show on the wearer's sprite
public sealed class GetEquipmentVisualsEvent(EntityUid equipee, string slot) : EntityEventArgs
{
    public readonly EntityUid Equipee = equipee;
    public readonly string Slot = slot;
    /// <summary>
    ///     The layers that will be added to the entity that is wearing this item.
    /// </summary>
    /// <remarks>
    ///     Note that the actual ordering of the layers depends on the order in which they are added to this list;
    /// </remarks>
    public List<(string, PrototypeLayerData)> Layers = new();
}

/// <summary>
///     Raised directed at a piece of clothing after its visuals have been updated.
/// </summary>
/// <remarks>
///     Useful for systems/components that modify the visual layers that an item adds to a player. (e.g. RGB memes)
/// </remarks>
public sealed class EquipmentVisualsUpdatedEvent(EntityUid equipee, string slot, HashSet<string> revealedLayers) : EntityEventArgs
{
    public readonly EntityUid Equipee = equipee;
    public readonly string Slot = slot;
    /// The layers that this item is now revealing.
    public HashSet<string> RevealedLayers = revealedLayers;
}

public sealed partial class ToggleMaskEvent : InstantActionEvent { }

/// <summary>
///     Event raised on the mask entity when it is toggled.
/// </summary>
[ByRefEvent]
public readonly record struct ItemMaskToggledEvent(EntityUid Wearer, string? equippedPrefix, bool IsToggled, bool IsEquip);

/// <summary>
///     Event raised on the entity wearing the mask when it is toggled.
/// </summary>
[ByRefEvent]
public readonly record struct WearerMaskToggledEvent(bool IsToggled);

/// <summary>
/// Raised on the clothing entity when it is equipped to a valid slot,
/// as determined by <see cref="ClothingComponent.Slots"/>.
/// </summary>
[ByRefEvent]
public readonly record struct ClothingGotEquippedEvent(EntityUid Wearer, ClothingComponent Clothing);

/// <summary>
/// Raised on the clothing entity when it is unequipped from a valid slot,
/// as determined by <see cref="ClothingComponent.Slots"/>.
/// </summary>
[ByRefEvent]
public readonly record struct ClothingGotUnequippedEvent(EntityUid Wearer, ClothingComponent Clothing);

/// <summary>
/// Raised on an entity when they equip a clothing item to a valid slot,
/// as determined by <see cref="ClothingComponent.Slots"/>.
/// </summary>
[ByRefEvent]
public readonly record struct ClothingDidEquippedEvent(Entity<ClothingComponent> Clothing);

/// <summary>
/// Raised on an entity when they unequip a clothing item from a valid slot,
/// as determined by <see cref="ClothingComponent.Slots"/>.
/// </summary>
[ByRefEvent]
public readonly record struct ClothingDidUnequippedEvent(Entity<ClothingComponent> Clothing);
