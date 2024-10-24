using Robust.Shared.Configuration;

namespace Content.Server.Cargo.Systems
{
    public sealed partial class CargoSystem
    {
        [Dependency] private readonly IConfigurationManager _cfg = default!;
    }

    [CVarDefs]
    public sealed class CargoCVars
    {
        /// <summary>
        ///     Determines if a trade station spawns in its own FTL map.
        /// </summary>
        /// <remarks>
        ///     Set this to true if and only if you are disabling the normal spawning method.
        ///     Otherwise you get two trade stations.
        ///     This does NOTHING to the trade station that spawns on the default map.
        ///     To change that, in Resources/Prototypes/Entities/Stations/base.yml;
        ///      comment out the "trade:" section.
        /// </remarks>
        public static readonly CVarDef<bool> CreateCargoMap =
            CVarDef.Create("cargo.tradestation_spawns_in_ftl_map", false, CVar.SERVERONLY);
    }
}
