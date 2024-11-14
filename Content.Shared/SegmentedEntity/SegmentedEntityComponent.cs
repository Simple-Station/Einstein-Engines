/*
* Delta-V - This file is licensed under AGPLv3
* Copyright (c) 2024 Delta-V Contributors
* See AGPLv3.txt for details.
*/

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.SegmentedEntity
{
    /// <summary>
    /// Controls initialization of any Multi-segmented entity
    /// </summary>
    [RegisterComponent, NetworkedComponent]
    [AutoGenerateComponentState]
    public sealed partial class SegmentedEntityComponent : Component
    {
        /// <summary>
        /// A list of each UID attached to the Lamia, in order of spawn
        /// </summary>
        [DataField("segments")]
        [AutoNetworkedField]
        public List<NetEntity> Segments = new();

        /// <summary>
        /// A clamped variable that represents the number of segments to be spawned
        /// </summary>
        [DataField("numberOfSegments")]
        public int NumberOfSegments = 18;

        /// <summary>
        /// How wide the initial segment should be.
        /// </summary>
        [DataField("initialRadius")]
        public float InitialRadius = 0.3f;

        /// <summary>
        /// Texture of the segment.
        /// </summary>
        [DataField("texturePath", required: true)]
        public string TexturePath;

        /// <summary>
        /// If UseTaperSystem is true, this constant represents the rate at which a segmented entity will taper towards the tip. Tapering is on a logarithmic scale, and will asymptotically approach 0.
        /// </summary>
        [DataField("offsetConstant")]
        public float OffsetConstant = 1.03f;

        /// <summary>
        /// Represents the prototype used to parent all segments
        /// </summary>
        [DataField("initialSegmentId")]
        public string InitialSegmentId = "LamiaInitialSegment";

        /// <summary>
        /// Represents the segment prototype to be spawned
        /// </summary>
        [DataField("segmentId")]
        public string SegmentId = "LamiaSegment";

        /// <summary>
        /// How much to slim each successive segment.
        /// </summary>
        [DataField("slimFactor")]
        public float SlimFactor = 0.93f;

        /// <summary>
        /// Set to 1f for constant width
        /// </summary>
        [DataField("useTaperSystem")]
        public bool UseTaperSystem = true;

        /// <summary>
        /// The standard distance between the centerpoint of each segment.
        /// </summary>
        [DataField("staticOffset")]
        public float StaticOffset = 0.15f;

        /// <summary>
        /// The standard sprite scale of each segment.
        /// </summary>
        [DataField("staticScale")]
        public float StaticScale = 1f;

        /// <summary>
        /// Used to more finely tune how much damage should be transfered from tail to body.
        /// </summary>
        [DataField("damageModifierOffset")]
        public float DamageModifierOffset = 0.4f;

        /// <summary>
        /// A clamped variable that represents how far from the tip should tapering begin.
        /// </summary>
        [DataField("taperOffset")]
        public int TaperOffset = 18;

        /// <summary>
        /// Coefficient used to finely tune how much explosion damage should be transfered to the body. This is calculated multiplicatively with the derived damage modifier set.
        /// </summary>
        [DataField("explosiveModifierOffset")]
        public float ExplosiveModifierOffset = 0.1f;

        [DataField("bulletPassover")]
        public bool BulletPassover = true;
    }
}
