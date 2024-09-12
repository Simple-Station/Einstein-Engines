using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Content.Shared.DeltaV.Harpy.Events;
using Content.Shared.DeltaV.Harpy.Components;

namespace Content.Client.DeltaV.Harpy;

/// <summary>
/// Handles offsetting an entity while flying
/// </summary>
public abstract class FlyingVisualizerSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    private ShaderInstance _shader = default!;
    public override void Initialize()
    {
        base.Initialize();

        _shader = _protoMan.Index<ShaderPrototype>("Wave").InstanceUnique();

        SubscribeLocalEvent<FlyingVisualsComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<FlyingVisualsComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<FlyingVisualsComponent, BeforePostShaderRenderEvent>(OnBeforeShaderPost);
    }

    private void OnStartup(EntityUid uid, FlyingVisualsComponent comp, ref ComponentStartup args)
    {
        _shader = _protoMan.Index<ShaderPrototype>("Wave").InstanceUnique();
        comp.Offset = _random.NextFloat(0, 1000);
        SetShader(uid, _shader);
    }

    private void OnShutdown(EntityUid uid, FlyingVisualsComponent comp, ref ComponentShutdown args)
    {
        SetShader(uid, null);
    }

    private void SetShader(Entity<SpriteComponent?> entity, ShaderInstance? instance)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return;

        entity.Comp.PostShader = instance;
        entity.Comp.GetScreenTexture = instance is not null;
        entity.Comp.RaiseShaderEvent = instance is not null;
    }

    private void OnBeforeShaderPost(EntityUid uid, FlyingVisualsComponent comp, ref BeforePostShaderRenderEvent args)
    {
        _shader.SetParameter("Speed", comp.Speed);
        _shader.SetParameter("Dis", comp.Dis);
        _shader.SetParameter("Offset", comp.Offset);
    }
}
