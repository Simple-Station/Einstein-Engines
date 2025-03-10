using Content.Server.Atmos.Components;
using System.Numerics;
using Robust.Shared.Map.Components;

namespace Content.Server.Atmos.EntitySystems;

// WELCOME TO THE MATRIX AIRFLOW SYSTEM.
public sealed partial class AtmosphereSystem
{
# pragma warning disable IDE1006
    /// <summary>
    ///     The standard issue "Search Pattern" used by the Matrix Airflow System.
    /// </summary>
    private List<(int, int)> MASSearchPattern = new List<(int, int)>
    {
        (-1,1),  (0,1),  (1,1),
        (-1,0),          (1,0),
        (-1,-1), (0,-1), (1,-1)
    };
# pragma warning restore IDE1006

    /// <summary>
    ///     A helper function from MAS that allows for (partially) converting Monstermos Tiles to MAS Vectors.
    ///     It returns a vector representing the flow direction of air passing over a tile, as described by Laplace's Equations.
    ///     The equations here are simplified however, and are omitting the matrix subdivisions.
    /// </summary>
    /// <remarks>
    ///     This function assumes you've already checked if tile.Air is null.
    /// </remarks>
    public Vector2 GetPressureVectorFromTile(GridAtmosphereComponent gridAtmos, TileAtmosphere tile)
    {
        if (!HasComp<MapGridComponent>(tile.GridIndex))
            return new Vector2(0, 0);

        var pressureVector = new Vector2(0, 0);
        foreach (var (x, y) in MASSearchPattern)
        {
            if (!gridAtmos.Tiles.TryGetValue(tile.GridIndices + (x, y), out var tileAtmosphere)
                || tileAtmosphere.Air is null
                || tileAtmosphere.PressureDirection is Shared.Atmos.AtmosDirection.Invalid)
                continue;

            var pressureDiff = tile.Air!.Pressure - tileAtmosphere.Air.Pressure;
            pressureVector += new Vector2(x, y) * pressureDiff;
        }
        return pressureVector;
    }
}
