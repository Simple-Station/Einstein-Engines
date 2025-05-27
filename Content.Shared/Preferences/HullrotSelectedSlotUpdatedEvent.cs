using Robust.Shared.Player;


namespace Content.Shared.Preferences;


[ByRefEvent]
public readonly record struct HullrotSelectedSlotUpdated(ICommonSession session, int slotIndex);
