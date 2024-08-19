using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Robust.Client.GameObjects;

namespace Content.Shared.DeltaV.Harpy
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

            var wingLayer = GetHarpyWingLayer(uid, sprite);
            if (wingLayer == null)
                return;
            sprite.LayerSetColor(wingLayer.Value, Color.White);
        }

        public int? GetHarpyWingLayer(EntityUid harpyUid, SpriteComponent? sprite = null)
        {
            if (!Resolve(harpyUid, ref sprite))
                return null;

            int index = 0;
            foreach (var layer in sprite.AllLayers)
            {
                // This feels like absolute shitcode, isn't there a better way to check for it?
                if (layer.Rsi?.Path.ToString() == "/Textures/Mobs/Customization/Harpy/harpy_wings.rsi")
                {
                    return index;
                }
                index++;
            }

            return null;
        }
    }
}