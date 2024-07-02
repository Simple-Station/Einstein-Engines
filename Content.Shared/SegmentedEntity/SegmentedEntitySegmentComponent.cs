/*
* Delta-V - This file is licensed under AGPLv3
* Copyright (c) 2024 Delta-V Contributors
* See AGPLv3.txt for details.
*/

using Robust.Shared.GameStates;

namespace Content.Shared.SegmentedEntity
{
    /// <summary>
    /// Lamia segment
    /// </summary>
    [RegisterComponent]
    [NetworkedComponent]
    public sealed partial class SegmentedEntitySegmentComponent : Component
    {
        [DataField("AttachedToUid")]
        public EntityUid AttachedToUid = default!;
        public float DamageModifyFactor = default!;
        public float OffsetSwitching = default!;
        public float ScaleFactor = default!;
        [DataField("DamageModifierCoefficient")]
        public float DamageModifierCoefficient = default!;
        public float ExplosiveModifyFactor = default!;
        public float OffsetConstant = default!;
        [DataField("Lamia")]
        public EntityUid Lamia = default!;
        public int MaxSegments = default!;
        public int SegmentNumber = default!;
        public float DamageModifierConstant = default!;
        [DataField("segmentId")]
        public string? segmentId;
    }
}
