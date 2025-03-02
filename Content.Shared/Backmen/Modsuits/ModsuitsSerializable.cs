using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Backmen.ModSuits;

[Serializable, NetSerializable]
public enum ModSuitUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class ModSuitUiMessage : BoundUserInterfaceMessage
{
    public NetEntity AttachedClothingUid;

    public ModSuitUiMessage(NetEntity attachedClothingUid)
    {
        AttachedClothingUid = attachedClothingUid;
    }
}

public sealed partial class ToggleModPartEvent : InstantActionEvent
{
}

[Serializable, NetSerializable]
public sealed partial class TogglePartDoAfterEvent : SimpleDoAfterEvent
{
}

/// <summary>
///     Event raises on modsuit when someone trying to toggle it
/// </summary>
public sealed class EquipModClothingAttemptEvent : CancellableEntityEventArgs
{
    public EntityUid User { get; }
    public EntityUid Target { get; }

    public EquipModClothingAttemptEvent(EntityUid user, EntityUid target)
    {
        User = user;
        Target = target;
    }
}

/// <summary>
///     Event raises on modsuit when someone trying to toggle it
/// </summary>
public sealed class UnequipModClothingAttemptEvent : CancellableEntityEventArgs
{
    public EntityUid User { get; }
    public EntityUid Target { get; }

    public UnequipModClothingAttemptEvent(EntityUid user, EntityUid target)
    {
        User = user;
        Target = target;
    }
}

[Serializable, NetSerializable]
public enum ModSuitVisualizerKeys : byte
{
    ClothingPieces,
}


[Serializable, NetSerializable]
public sealed class ModSuitVisualizerGroupData : ICloneable
{
    public List<NetEntity> PieceList;
    public List<NetEntity> AttachedPieces;

    public ModSuitVisualizerGroupData(List<NetEntity> pieceList, List<NetEntity> attachedPieces)
    {
        PieceList = pieceList;
        AttachedPieces = attachedPieces;
    }

    public object Clone()
    {
        return new ModSuitVisualizerGroupData(new List<NetEntity>(PieceList), new List<NetEntity>(AttachedPieces));
    }
}

/// <summary>
/// Status of modsuit attached entities
/// </summary>
[Serializable, NetSerializable]
public enum ModSuitAttachedStatus : byte
{
    NoneToggled,
    PartlyToggled,
    AllToggled,
}

