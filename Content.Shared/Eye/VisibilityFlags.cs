using Robust.Shared.Serialization;

namespace Content.Shared.Eye
{
    [Flags]
    [FlagsFor(typeof(VisibilityMaskLayer))]
    public enum VisibilityFlags : int
    {
        None = 0,
        Normal = 1 << 0,
        Ghost = 1 << 1,
        Subfloor = 1 << 2,
        PsionicInvisibility = 1 << 3, //Nyano - Summary: adds Psionic Invisibility as a visibility layer. Currently does nothing.
        Ethereal = 1 << 3,
        TelegnosticProjection = 5,
        Abductor = 1 << 10, // I HATE BITWISE OPERATIONS!! GRAAAAH
    }
}
