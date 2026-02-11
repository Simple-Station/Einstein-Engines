using Content.Client.Alerts;
using Content.Shared.Pinpointer;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;

namespace Content.Client.Pinpointer;

public sealed class PinpointerSystem : SharedPinpointerSystem
{
    [Dependency] private readonly IEyeManager _eyeManager = default!;

    // WD EDIT START
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PinpointerComponent, UpdateAlertSpriteEvent>(OnUpdateAlertSprite);
    }

    private void OnUpdateAlertSprite(EntityUid uid, PinpointerComponent component, ref UpdateAlertSpriteEvent args)
    {
        if (args.Alert.ID != component.Alert)
            return;

        var sprite = args.SpriteViewEnt.Comp;
        var eye = _eyeManager.CurrentEye;
        var angle = component.DistanceToTarget switch
        {
            Distance.Close or Distance.Medium or Distance.Far => component.ArrowAngle + eye.Rotation,
            _ => Angle.Zero
        };

        sprite.LayerSetRotation(PinpointerLayers.Screen, angle);
        sprite.LayerSetState(PinpointerLayers.Screen, component.DistanceToTarget.ToString().ToLower());
    }
    // WD EDIT END

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // we want to show pinpointers arrow direction relative
        // to players eye rotation (like it was in SS13)

        // because eye can change it rotation anytime
        // we need to update this arrow in a update loop
        var query = EntityQueryEnumerator<PinpointerComponent, SpriteComponent>();
        while (query.MoveNext(out var _, out var pinpointer, out var sprite))
        {
            if (!pinpointer.HasTarget || !sprite.LayerExists(PinpointerLayers.Screen)) // WD EDIT
                continue;
            var eye = _eyeManager.CurrentEye;
            var angle = pinpointer.ArrowAngle + eye.Rotation;

            switch (pinpointer.DistanceToTarget)
            {
                case Distance.Close:
                case Distance.Medium:
                case Distance.Far:
                    sprite.LayerSetRotation(PinpointerLayers.Screen, angle);
                    break;
                default:
                    sprite.LayerSetRotation(PinpointerLayers.Screen, Angle.Zero);
                    break;
            }
        }
    }
}
