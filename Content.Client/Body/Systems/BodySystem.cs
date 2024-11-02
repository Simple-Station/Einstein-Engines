using Content.Shared.Body.Systems;
using Content.Shared.Body.Part;
using Robust.Client.GameObjects;

namespace Content.Client.Body.Systems;

public sealed class BodySystem : SharedBodySystem
{
    protected override void UpdateAppearance(EntityUid uid, BodyPartAppearanceComponent component)
    {
        if (TryComp(uid, out SpriteComponent? sprite))
        {
            if (component.Color != null)
            {
                //TODO a few things need to be adjusted before this is ready to be used - also need to find a way to update the player sprite
                //sprite.Color = component.Color.Value;
            }
        }
    }
}
