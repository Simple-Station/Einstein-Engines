// SPDX-FileCopyrightText: 2022 Nemanja
// SPDX-FileCopyrightText: 2023 DrSmugleaf
// SPDX-FileCopyrightText: 2023 Leon Friedrich
// SPDX-FileCopyrightText: 2023 keronshb
// SPDX-FileCopyrightText: 2025 ScyronX
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.DoAfter;
using Content.Shared.Mech.Components;
using Robust.Shared.Serialization;

namespace Content.Shared.Mech.Equipment.Components;

/// <summary>
/// A piece of equipment that can be installed into <see cref="MechComponent"/>
/// </summary>
[RegisterComponent]
public sealed partial class MechEquipmentComponent : Component
{
    /// <summary>
    /// How long does it take to install this piece of equipment
    /// </summary>
    [DataField("installDuration")] public float InstallDuration = 5;

    /// <summary>
    /// The mech that the equipment is inside of.
    /// </summary>
    [ViewVariables] public EntityUid? EquipmentOwner;

    /// <summary>
    /// Monolith: Equipment that cant be fired on grids.
    /// </summary>
    [DataField]
    public bool PreventFireOnGrid = false;
}

/// <summary>
/// Raised on the equipment when the installation is finished successfully
/// </summary>
public sealed class MechEquipmentInstallFinished : EntityEventArgs
{
    public EntityUid Mech;

    public MechEquipmentInstallFinished(EntityUid mech)
    {
        Mech = mech;
    }
}

/// <summary>
/// Raised on the equipment when the installation fails.
/// </summary>
public sealed class MechEquipmentInstallCancelled : EntityEventArgs
{
}

[Serializable, NetSerializable]
public sealed partial class GrabberDoAfterEvent : SimpleDoAfterEvent
{
}

[Serializable, NetSerializable]
public sealed partial class InsertEquipmentEvent : SimpleDoAfterEvent
{
}

