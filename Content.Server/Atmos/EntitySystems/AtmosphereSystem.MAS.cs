using Content.Server.Atmos.Components;
using System.Numerics;
using Robust.Shared.Map.Components;
using Content.Shared.Atmos;

namespace Content.Server.Atmos.EntitySystems;

// WELCOME TO THE MATRIX AIRFLOW SYSTEM.
public sealed partial class AtmosphereSystem
{
# pragma warning disable IDE1006
    /// <summary>
    ///     The standard issue "Search Pattern" used by the Matrix Airflow System.
    /// </summary>
    private readonly List<(int, int, AtmosDirection)> MASSearchPattern = new List<(int, int, AtmosDirection)>
    {
        (-1,1, AtmosDirection.SouthEast),  (0,1, AtmosDirection.South),  (1,1, AtmosDirection.SouthWest),
        (-1,0, AtmosDirection.East),                                      (1,0, AtmosDirection.West),
        (-1,-1, AtmosDirection.NorthEast), (0,-1, AtmosDirection.North), (1,-1, AtmosDirection.NorthWest)
    };
# pragma warning restore IDE1006

    /// <summary>
    ///     This function solves for the flow of air across a given tile, expressed in the format of (Vector) kg/ms^2.
    ///     Multiply this output against any "Area"(such as a human cross section) in the form of meters squared to get the force of air flowing against that object in Newtons.
    ///     From there, you can divide by the object's mass (in kg) to get the object's acceleration in meters per second squared.
    ///     To solve for the object's change in velocity per CPU tick, you then multiply by frameTime to get Delta-V.
    /// </summary>
    /// <remarks>
    ///     This function is a direct implementation of the Navier-Stokes system of partial differential equations.
    ///     Simplified since we don't need to account for fluid viscosity(yet) as this is currently only being used to handle breathable atmosphere.
    /// </remarks>
    public Vector2 GetPressureVectorFromTile(GridAtmosphereComponent gridAtmos, TileAtmosphere tile)
    {
        if (!HasComp<MapGridComponent>(tile.GridIndex))
            return new Vector2(0, 0);

        var centerPressure = tile.Air?.Pressure ?? 0f;
        var pressureVector = new Vector2(0, 0);
        foreach (var (x, y, dir) in MASSearchPattern)
        {
            var offsetVector = new Vector2(x, y);
            // If the tile checked doesn't exist, or has no air, or it's space,
            // then there's nothing to "push back" against our center tile's air.
            if (!gridAtmos.Tiles.TryGetValue(tile.GridIndices + (x, y), out var tileAtmosphere)
                || tileAtmosphere.Space)
            {
                pressureVector += offsetVector * centerPressure;
                continue;
            }

            // If the tile checked is blocking airflow from this direction, the center tile's air "Bounces" off it and into the
            // opposite direction.
            if (tileAtmosphere.AirtightData.BlockedDirections is AtmosDirection.All
                || tileAtmosphere.AirtightData.BlockedDirections.HasFlag(dir)
                || tileAtmosphere.Air is null)
            {
                pressureVector -= offsetVector * centerPressure;
                continue;
            }

            // Center tile now transfers its pressure across the target.
            var pressureDiff = centerPressure - tileAtmosphere.Air.Pressure;
            pressureVector += offsetVector * pressureDiff;

            // And finally, the pressure in the target tile is resisting the original target pressure.
            pressureVector -= offsetVector * tileAtmosphere.Air.Pressure;
        }
        // from TCJ: By this point in the equation, all possible conditions are now checked, and for any airtight vessel with a standard atmosphere, the final output will be <0, 0>.
        // Should any holes exist in the ship, the air will now flow at an exponential rate towards it, while deflecting around walls.
        return pressureVector;
    }
}
