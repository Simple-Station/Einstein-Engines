using System.Numerics;
using Content.Shared._WoF.Flight;
using Robust.Client.GameObjects;

namespace Content.Client._WoF.Flight;

/// <summary>
///     Lifts a flying dragon's sprite according to its altitude so height reads at a glance.
///     TODO: ground shadow that stays on the tile, banking, and wind particles (see the flight design doc).
/// </summary>
public sealed class DragonFlightVisualsSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DragonFlightComponent, AfterAutoHandleStateEvent>(OnHandleState);
        SubscribeLocalEvent<DragonFlightComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnHandleState(Entity<DragonFlightComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        var height = ent.Comp.Altitude switch
        {
            FlightAltitude.Low => ent.Comp.LowVisualOffset,
            FlightAltitude.High => ent.Comp.HighVisualOffset,
            _ => 0f,
        };

        _sprite.SetOffset((ent.Owner, sprite), new Vector2(0f, height));
    }

    private void OnShutdown(Entity<DragonFlightComponent> ent, ref ComponentShutdown args)
    {
        if (TryComp<SpriteComponent>(ent, out var sprite))
            _sprite.SetOffset((ent.Owner, sprite), Vector2.Zero);
    }
}
