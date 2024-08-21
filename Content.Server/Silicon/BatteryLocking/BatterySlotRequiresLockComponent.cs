namespace Content.Server.Silicon.BatteryLocking;

[RegisterComponent]
public sealed partial class BatterySlotRequiresLockComponent : Component
{
    [DataField("cellSlotId"), ViewVariables(VVAccess.ReadWrite)]
    public string ItemSlot = string.Empty;
}
