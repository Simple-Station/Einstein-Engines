/*
* Delta-V - This file is licensed under AGPLv3
* Copyright (c) 2024 Delta-V Contributors
* See AGPLv3.txt for details.
*/

using Robust.Shared.Serialization;

namespace Content.Shared.SegmentedEntity
{
    [Serializable, NetSerializable]
    public enum SegmentedEntitySegmentVisualLayers
    {
        Tail,
        Armor,
        ArmorRsi,
    }
}
