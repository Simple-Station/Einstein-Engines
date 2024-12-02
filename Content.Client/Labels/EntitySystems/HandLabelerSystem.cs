#region

using Content.Client.Labels.UI;
using Content.Shared.Labels;
using Content.Shared.Labels.Components;
using Content.Shared.Labels.EntitySystems;

#endregion


namespace Content.Client.Labels.EntitySystems;


public sealed class HandLabelerSystem : SharedHandLabelerSystem
{
    protected override void UpdateUI(Entity<HandLabelerComponent> ent)
    {
        if (UserInterfaceSystem.TryGetOpenUi(ent.Owner, HandLabelerUiKey.Key, out var bui)
            && bui is HandLabelerBoundUserInterface cBui)
            cBui.Reload();
    }
}
