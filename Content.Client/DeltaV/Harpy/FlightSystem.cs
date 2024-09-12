using Robust.Client.GameObjects;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.DeltaV.Harpy;
using Content.Shared.DeltaV.Harpy.Components;
using Content.Shared.DeltaV.Harpy.Events;

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
            var uid = GetEntity(args.Uid);
            if (!_entityManager.TryGetComponent(uid, out SpriteComponent? sprite))
                return;

            if (args.Layer != string.Empty)
            {
                var targetLayer = GetAnimatedLayer(uid, args.Layer, sprite);
                if (targetLayer == null)
                    return;
                sprite.LayerSetColor(targetLayer.Value, Color.White);
            }

            if (args.IsFlying && args.IsAnimated && args.AnimationKey != "default")
            {
                var comp = new FlyingVisualsComponent
                {
                    AnimationKey = args.AnimationKey,
                };
                AddComp(uid, comp, true);
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