using System.Numerics;
using Content.Client.Parallax.Managers;
using Content.Shared.CCVar;
using Content.Shared.Parallax;
using Content.Shared.Parallax.Biomes;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client.Parallax;

public sealed class ParallaxOverlay : Overlay
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IParallaxManager _manager = default!;
    private readonly ParallaxSystem _parallax;
    private readonly MapSystem _map;

    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowWorld;

    private TimeSpan _lastUpdate = TimeSpan.Zero;

    public ParallaxOverlay()
    {
        ZIndex = ParallaxSystem.ParallaxZIndex;
        IoCManager.InjectDependencies(this);
        _parallax = _entManager.System<ParallaxSystem>();
        _map = _entManager.System<MapSystem>();
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (args.MapId == MapId.Nullspace || _entManager.HasComponent<BiomeComponent>(_mapManager.GetMapEntityId(args.MapId)))
            return false;

        return true;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        TimeSpan lastUpdate = _lastUpdate == TimeSpan.Zero ? _timing.CurTime : _lastUpdate;
        float deltaTime = (float) (_timing.CurTime - lastUpdate).TotalSeconds;
        _lastUpdate = _timing.CurTime;

        if (args.MapId == MapId.Nullspace || !_map.TryGetMap(args.MapId, out var mapUid))
            return;

        if (!_configurationManager.GetCVar(CCVars.ParallaxEnabled))
            return;

        ParallaxComponent? parallax = _entManager.GetComponentOrNull<ParallaxComponent>(_playerManager.LocalEntity);
        parallax ??= _entManager.GetComponentOrNull<ParallaxComponent>(mapUid);

        if (parallax == null)
        {
            DrawLayers(args, _parallax.GetParallaxLayers(ParallaxSystem.Fallback), 1);
            return;
        }

        float alpha = parallax.IsSwapping ? parallax.SwapTimer / parallax.SwapDuration : 1f;
        DrawLayers(args, _parallax.GetParallaxLayers(parallax.Parallax), alpha);
        if (parallax.IsSwapping)
        {
            DrawLayers(args, _parallax.GetParallaxLayers(parallax.SwappedParallax!), 1f - alpha);
            parallax.SwapTimer += deltaTime;

            if (parallax.SwapTimer > parallax.SwapDuration)
            {
                parallax.SwappedParallax = null;
                parallax.SwapTimer = parallax.SwapDuration = 0;
            }
        }
    }

    private void DrawLayers(OverlayDrawArgs args, ParallaxLayerPrepared[] layers, float alpha)
    {
        var position = args.Viewport.Eye?.Position.Position ?? Vector2.Zero;
        var worldHandle = args.WorldHandle;
        var realTime = (float) _timing.RealTime.TotalSeconds;

        foreach (var layer in layers)
        {
            ShaderInstance? shader;

            if (!string.IsNullOrEmpty(layer.Config.Shader))
                shader = _prototypeManager.Index<ShaderPrototype>(layer.Config.Shader).Instance();
            else
                shader = null;

            worldHandle.UseShader(shader);
            var tex = layer.Texture;

            // Size of the texture in world units.
            var size = (tex.Size / (float) EyeManager.PixelsPerMeter) * layer.Config.Scale;

            // The "home" position is the effective origin of this layer.
            // Parallax shifting is relative to the home, and shifts away from the home and towards the Eye centre.
            // The effects of this are such that a slowness of 1 anchors the layer to the centre of the screen, while a slowness of 0 anchors the layer to the world.
            // (For values 0.0 to 1.0 this is in effect a lerp, but it's deliberately unclamped.)
            // The ParallaxAnchor adapts the parallax for station positioning and possibly map-specific tweaks.
            var home = layer.Config.WorldHomePosition + _manager.ParallaxAnchor;
            var scrolled = layer.Config.Scrolling * realTime;

            // Origin - start with the parallax shift itself.
            var originBL = (position - home) * layer.Config.Slowness + scrolled;

            // Place at the home.
            originBL += home;

            // Adjust.
            originBL += layer.Config.WorldAdjustPosition;

            // Centre the image.
            originBL -= size / 2;

            if (layer.Config.Tiled)
            {
                // Remove offset so we can floor.
                var flooredBL = args.WorldAABB.BottomLeft - originBL;

                // Floor to background size.
                flooredBL = (flooredBL / size).Floored() * size;

                // Re-offset.
                flooredBL += originBL;

                for (var x = flooredBL.X; x < args.WorldAABB.Right; x += size.X)
                {
                    for (var y = flooredBL.Y; y < args.WorldAABB.Top; y += size.Y)
                    {
                        worldHandle.DrawTextureRect(tex, Box2.FromDimensions(new Vector2(x, y), size), Color.White.WithAlpha(alpha));
                    }
                }
            }
            else
            {
                worldHandle.DrawTextureRect(tex, Box2.FromDimensions(originBL, size), Color.White.WithAlpha(alpha));
            }
        }

        worldHandle.UseShader(null);
    }
}

