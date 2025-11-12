using Content.Client._Crescent.Blocking.Components;
using Content.Shared._Crescent.Blocking;
using Robust.Shared.Prototypes;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;

namespace Content.Client._Crescent.Blocking;

public sealed class BlockingVisualsSystem : SharedBlockingSystem
{
    [Dependency] private readonly IPrototypeManager _protoMan = default!;

    private ShaderInstance _shader = default!;

    public override void Initialize()
    {
        base.Initialize();

        _shader = _protoMan.Index<ShaderPrototype>("ShieldingOutline").InstanceUnique(); // those who hardcode... ts tuff...

        SubscribeLocalEvent<BlockingVisualsComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<BlockingVisualsComponent, ComponentShutdown>(OnShutdown);
    }

    private void SetShader(EntityUid uid, bool enabled, BlockingVisualsComponent? component = null, SpriteComponent? sprite = null)
    {
        if (!Resolve(uid, ref component, ref sprite, false))
            return;

        sprite.PostShader = enabled ? _shader : null;
        sprite.GetScreenTexture = enabled;
        sprite.RaiseShaderEvent = enabled;
    }
    private void OnStartup(EntityUid uid, BlockingVisualsComponent component, ComponentStartup args)
    {
        SetShader(uid, component.Enabled, component);
    }

    private void OnShutdown(EntityUid uid, BlockingVisualsComponent component, ComponentShutdown args)
    {
        if (!Terminating(uid))
            SetShader(uid, false, component);
    }
}
