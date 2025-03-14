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
public enum ModSuitDeployUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class ModSuitBuiState : BoundUserInterfaceState
{
    public float ChargePercent;

    public int CurrentComplexity;

    public bool HasBattery;

    public List<(NetEntity, bool)> Modules;

    public ModSuitBuiState(float chargePercent, int currentComplexity, bool hasBattery, List<(NetEntity, bool)> modules)
    {
        ChargePercent = chargePercent;
        CurrentComplexity = currentComplexity;
        HasBattery = hasBattery;
        Modules = modules;
    }
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

[Serializable, NetSerializable]
public sealed class TogglePartModulesUiMessage : BoundUserInterfaceMessage
{
    public NetEntity AttachedClothingUid;

    public TogglePartModulesUiMessage(NetEntity attachedClothingUid)
    {
        AttachedClothingUid = attachedClothingUid;
    }
}

[Serializable, NetSerializable]
public sealed class ToggleModuleUiMessage : BoundUserInterfaceMessage
{
    public NetEntity ModuleUid;

    public ToggleModuleUiMessage(NetEntity moduleUid)
    {
        ModuleUid = moduleUid;
    }
}

public sealed partial class ToggleModEvent : InstantActionEvent
{
}

public sealed partial class ActivateModEvent : InstantActionEvent
{
}

[Serializable, NetSerializable]
public sealed partial class TogglePartDoAfterEvent : SimpleDoAfterEvent
{
}

[Serializable, NetSerializable]
public sealed partial class ToggleModuleDoAfterEvent : SimpleDoAfterEvent
{
}

[Serializable, NetSerializable]
public sealed partial class TogglePartModulesDoAfterEvent : SimpleDoAfterEvent
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
    Modules,
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

