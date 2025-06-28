using Content.Shared.Inventory;
using JetBrains.Annotations;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Content.Server.Station.Systems;
using Content.Server.Power.Components;
using Content.Server.Factory.Components;
using Content.Shared.Physics;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Physics.Components;
using Content.Server.DeviceLinking.Events;
using Content.Server.DeviceLinking.Systems;
using System.Linq;
using Content.Server.Stack;
using Content.Shared.Stacks;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.GameObjects;
using Robust.Server.GameObjects;
using Content.Server.LocalizedRadio.Prototypes;
using Content.Server.Radio.EntitySystems;
using Content.Server.Radio;
using Content.Server.Radio.Components;
using SixLabors.ImageSharp.Formats.Png;
using System.Numerics;
using Content.Shared.Radio.Components;
using Content.Shared.Radio;

namespace Content.Server.LocalizedRadio.EntitySystems
{


    [UsedImplicitly]
    public sealed partial class LocalizedRadioSystem : EntitySystem
    {

        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly TransformSystem _transformSystem = default!;
        internal Dictionary<string, float> ForceLocalize = new();

        public override void Initialize()
        {
            base.Initialize();

            foreach (var localized in _prototypeManager.EnumeratePrototypes<RadioChannelPrototype>().ToArray())
            {
                if (localized.LocalizedRange == 0)
                    continue;
                ForceLocalize.Add(localized.ID, localized.LocalizedRange);
            }

            if (ForceLocalize.Count > 0)
            {
                SubscribeLocalEvent<RadioReceiveAttemptEvent>(RadioReceive);
            }

        }

        private void RadioReceive(ref RadioReceiveAttemptEvent args)
        {
            if (!ForceLocalize.ContainsKey(args.Channel.ID))
                return;
            Vector2 distance = _transformSystem.GetWorldPosition(args.RadioReceiver) - _transformSystem.GetWorldPosition(args.RadioSource);
            if (Math.Abs(distance.Length()) > ForceLocalize[args.Channel.ID])
                args.Cancelled = true;

        }
    }
}

