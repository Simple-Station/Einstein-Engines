namespace Content.Shared._White.Inventory;

public sealed partial class WhiteInventorySystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        InitializeEquip();
    }
}
