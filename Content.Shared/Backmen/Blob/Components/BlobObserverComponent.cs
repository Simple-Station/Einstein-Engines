using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared.Backmen.Blob.Components;

[RegisterComponent]
public sealed partial class BlobObserverControllerComponent : Component
{
    public Entity<BlobObserverComponent> Blob;
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BlobObserverComponent : Component
{
    [ViewVariables]
    public bool IsProcessingMoveEvent;

    [ViewVariables]
    public Entity<BlobCoreComponent>? Core = default!;

    [ViewVariables]
    public bool CanMove = true;

    [ViewVariables, AutoNetworkedField]
    public BlobChemType SelectedChemId = BlobChemType.ReactiveSpines;

    public override bool SendOnlyToOwner => true;

    [ViewVariables, AutoNetworkedField]
    public EntityUid VirtualItem = EntityUid.Invalid;
}

[Serializable, NetSerializable]
public sealed class BlobChemSwapBoundUserInterfaceState(
    BlobChemColors chemList,
    BlobChemType selectedId)
    : BoundUserInterfaceState
{
    public readonly BlobChemColors ChemList = chemList;
    public readonly BlobChemType SelectedChem = selectedId;
}

[Serializable, NetSerializable]
public sealed class BlobChemSwapPrototypeSelectedMessage(BlobChemType selectedId) : BoundUserInterfaceMessage
{
    public readonly BlobChemType SelectedId = selectedId;
}

[Serializable, NetSerializable]
public enum BlobChemSwapUiKey : byte
{
    Key
}

/// <summary>
/// Tries to transform the Target blob tile in other type, making checks for Node and/or similar tiles.
/// </summary>
public sealed partial class BlobTransformTileActionEvent : WorldTargetActionEvent
{
    /// <summary>
    /// Type of tile that can be transformed.
    /// Will be ignored if equals to Invalid.
    /// </summary>
    [DataField]
    public BlobTileType TransformFrom = BlobTileType.Normal;

    /// <summary>
    /// Type of the resulting tile.
    /// </summary>
    [DataField]
    public BlobTileType TileType = BlobTileType.Invalid;

    /// <summary>
    /// If specified, tries to find a blob node
    /// in given radius and returns back if failed.
    /// </summary>
    [DataField]
    public float? NodeSearchRadius;

    /// <summary>
    /// If specified, tries to find a tile of the same type
    /// in given radius and returns back if failed.
    /// </summary>
    [DataField]
    public float? TileSearchRadius;

    public BlobTransformTileActionEvent(EntityUid performer, EntityCoordinates target, BlobTileType transformFrom, BlobTileType tileType)
    {
        Performer = performer;
        Target = target;
        TransformFrom = transformFrom;
        TileType = tileType;
    }
}

public sealed partial class BlobCreateBlobbernautActionEvent : WorldTargetActionEvent;
public sealed partial class BlobSplitCoreActionEvent : WorldTargetActionEvent;
public sealed partial class BlobSwapCoreActionEvent : WorldTargetActionEvent;
public sealed partial class BlobToCoreActionEvent : InstantActionEvent;
public sealed partial class BlobSwapChemActionEvent : InstantActionEvent;
