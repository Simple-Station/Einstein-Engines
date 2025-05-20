using Robust.Shared.Enums;

namespace Content.Shared._EE.GenderChange
{
    public record struct GenderChangeEvent(EntityUid Uid, Gender Gender);
}
