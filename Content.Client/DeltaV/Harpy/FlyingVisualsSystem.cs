using Content.Client.DeltaV.Harpy.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Client.DeltaV.Harpy;

/// <summary>
/// Handles offsetting an entity while flying
/// </summary>
public sealed class FlyingVisualizerSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FlyingVisualsComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<FlyingVisualsComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<FlyingVisualsComponent, BeforePostShaderRenderEvent>(OnBeforeShaderPost);
    }

    private void OnStartup(EntityUid uid, FlyingVisualsComponent comp, ComponentStartup args)
    {
        comp.Shader = _protoMan.Index<ShaderPrototype>(comp.AnimationKey).InstanceUnique();
        //comp.Rotation = _random.NextFloat(0, 1000);
        Logger.Debug($"Executing OnStartup! with {comp.Shader}, {comp.AnimateLayer}, {comp.TargetLayer}");
        AddShader(uid, comp.Shader, comp.AnimateLayer, comp.TargetLayer);
    }

    private void OnShutdown(EntityUid uid, FlyingVisualsComponent comp, ComponentShutdown args)
    {
        Logger.Debug($"Executing OnShutdown! with {comp.Shader} changing to {null}, {comp.AnimateLayer}, {comp.TargetLayer}");
        AddShader(uid, null, comp.AnimateLayer, comp.TargetLayer);
    }

    private void AddShader(Entity<SpriteComponent?> entity, ShaderInstance? shader, bool animateLayer, int? layer)
    {
        Logger.Debug($"Executing AddShader on {entity}! with {shader}, {animateLayer}, {layer}");
        if (!Resolve(entity, ref entity.Comp, false))
            return;

        if (!animateLayer)
        {
            entity.Comp.PostShader = shader;
        }

        if (animateLayer && layer is not null)
        {
            Logger.Debug($"Executing LayerSetShader!");
            entity.Comp.LayerSetShader(layer.Value, shader);
        }

        entity.Comp.GetScreenTexture = shader is not null;
        entity.Comp.RaiseShaderEvent = shader is not null;
    }

    private void OnBeforeShaderPost(EntityUid uid, FlyingVisualsComponent comp, ref BeforePostShaderRenderEvent args)
    {
        comp.Shader.SetParameter("Speed", comp.Speed);
        comp.Shader.SetParameter("Dis", comp.Distance);
        comp.Shader.SetParameter("Offset", comp.Rotation);
    }
}
