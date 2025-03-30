using Content.Shared.Humanoid.Markings;
using Robust.Shared.Serialization;

namespace Content.Shared.Humanoid
{
    [Serializable, NetSerializable]
    public enum HumanoidVisualLayers : byte
    {
        Face,
        Tail,
        Wings,
        Hair,
        FacialHair,
        Chest,
        Underwear,
        Undershirt,
        Head,
        Snout,
        HeadSide, // side parts (i.e., frills)
        HeadTop,  // top parts (i.e., ears)
        Eyes,
        RArm,
        LArm,
        RHand,
        LHand,
        RLeg,
        LLeg,
        RFoot,
        LFoot,
        Handcuffs,
        StencilMask,
        Ensnare,
        Fire,
    }
}
