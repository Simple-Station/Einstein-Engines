using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{

    //
    // This is the real respawn timer that gets used ingame. it's also in minutes.
    //
    public static readonly CVarDef<float> GhostRespawnTime =
        CVarDef.Create("ghost.respawn_time", 10f, CVar.SERVERONLY);

    public static readonly CVarDef<int> GhostRespawnMaxPlayers =
        CVarDef.Create("ghost.respawn_max_players", 9999, CVar.SERVERONLY);


    //
    // This is checked at latejoin-time to determine whether you can join as the same character.
    //
    public static readonly CVarDef<bool> GhostAllowSameCharacter =
        CVarDef.Create("ghost.allow_same_character", true, CVar.SERVERONLY);
}
