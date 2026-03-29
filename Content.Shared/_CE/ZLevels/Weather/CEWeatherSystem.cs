/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Content.Shared._CE.ZLevels.Core.Components;
using Content.Shared.Weather;
using Robust.Shared.Map.Components;

namespace Content.Shared._CE.ZLevels.Weather;

/// <summary>
/// A subsystem that connects WeatherSystem with ZLevelSystem. Allows you to control the weather for the entire z-network at once.
/// </summary>
public sealed class CEWeatherSystem : EntitySystem
{
    [Dependency] private readonly SharedWeatherSystem _weather = default!;

    public void SetWeather(Entity<CEZLevelsNetworkComponent?> network, WeatherPrototype? proto, TimeSpan? endTime)
    {
        if (!Resolve(network, ref network.Comp))
            return;

        foreach (var (_, map) in network.Comp.ZLevels)
        {
            if (!TryComp<MapComponent>(map, out var mapComp))
                continue;

            _weather.SetWeather(mapComp.MapId, proto, endTime);
        }
    }
}
