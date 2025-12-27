using Content.Shared.GameTicking;
using Robust.Client.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Content.Shared._Lavaland.Mobs.Components;

#pragma warning disable CS0618

namespace Content.Client._Lavaland.Mobs.Bosses;

public sealed class MegafaunaVisualSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _spriteSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MegafaunaVisualComponent, AfterAutoHandleStateEvent>(OnState);
    }

    private void OnState(EntityUid uid, MegafaunaVisualComponent comp, ref AfterAutoHandleStateEvent args)
    {
        if (comp.SpriteState == null) return;

        //_spriteSystem.LayerSetSprite(uid, 0,
        //    new SpriteSpecifier.Rsi(new ResPath("/Textures/_Lavaland/Mobs/Bosses/vigilante.rsi"), comp.SpriteState));
        var sprite = EntityManager.GetComponent<SpriteComponent>(uid);
        sprite.LayerSetState(0, comp.SpriteState);
    }
}
