using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Silicon;

[Serializable, NetSerializable]
public sealed partial class BatteryDrinkerDoAfterEvent : SimpleDoAfterEvent
{
    public BatteryDrinkerDoAfterEvent() { }
}
