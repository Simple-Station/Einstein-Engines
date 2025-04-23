using Content.Server.Kitchen.Components;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;


namespace Content.Server.Item;

/// <summary>
/// Handles whether this item is sharp when toggled on.
/// </summary>
[RegisterComponent]
public sealed partial class ItemToggleSharpComponent : Component
{
}

public sealed class ServerItemToggleSystem : ItemToggleSystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<ItemToggleSharpComponent, ItemToggledEvent>(ToggleSharp);
    }

    private void ToggleSharp(Entity<ItemToggleSharpComponent> ent, ref ItemToggledEvent args)
    {
        // TODO generalize this into a  "ToggleComponentComponent", though probably with a better name
        if (args.Activated)
            EnsureComp<SharpComponent>(ent);
        else
            RemCompDeferred<SharpComponent>(ent);
    }


}
