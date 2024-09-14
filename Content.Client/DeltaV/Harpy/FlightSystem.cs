using Robust.Client.GameObjects;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.DeltaV.Harpy;
using Content.Shared.DeltaV.Harpy.Components;
using Content.Shared.DeltaV.Harpy.Events;
using Content.Client.DeltaV.Harpy.Components;

namespace Content.Client.DeltaV.Harpy
{
    public sealed class FlightSystem : SharedFlightSystem
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        public override void Initialize()
        {
            base.Initialize();

            SubscribeNetworkEvent<FlightEvent>(OnFlight);

        }

        private void OnFlight(FlightEvent args)
        {
            Logger.Debug("Starting onFlight!");
            var uid = GetEntity(args.Uid);
            if (!_entityManager.TryGetComponent(uid, out SpriteComponent? sprite) || !args.IsAnimated)
                return;

            int? targetLayer = null;
            if (args.IsLayerAnimated && args.Layer != string.Empty)
            {
                targetLayer = GetAnimatedLayer(uid, args.Layer, sprite);
                if (targetLayer == null)
                    return;
            }

            if (args.IsFlying && args.IsAnimated && args.AnimationKey != "default")
            {
                var comp = new FlyingVisualsComponent
                {
                    AnimationKey = args.AnimationKey,
                    AnimateLayer = args.IsLayerAnimated,
                    TargetLayer = targetLayer,
                };
                AddComp(uid, comp);
            }
            if (!args.IsFlying)
            {
                RemComp<FlyingVisualsComponent>(uid);
            }
        }

        public int? GetAnimatedLayer(EntityUid uid, string targetLayer, SpriteComponent? sprite = null)
        {
            if (!Resolve(uid, ref sprite))
                return null;

            int index = 0;
            foreach (var layer in sprite.AllLayers)
            {
                // This feels like absolute shitcode, isn't there a better way to check for it?
                if (layer.Rsi?.Path.ToString() == targetLayer)
                {
                    return index;
                }
                index++;
            }

            return null;
        }
    }
}