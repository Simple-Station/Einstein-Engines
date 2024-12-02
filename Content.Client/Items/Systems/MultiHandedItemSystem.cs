#region

using Content.Shared.Hands;
using Content.Shared.Item;

#endregion


namespace Content.Client.Items.Systems;


public sealed class MultiHandedItemSystem : SharedMultiHandedItemSystem
{
    protected override void OnEquipped(EntityUid uid, MultiHandedItemComponent component, GotEquippedHandEvent args) { }

    protected override void OnUnequipped(
        EntityUid uid,
        MultiHandedItemComponent component,
        GotUnequippedHandEvent args
    ) { }
}
