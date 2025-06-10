using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{

    // 
    // THIS is the real respawn timer that gets used ingame. it's also in minutes
    // 
    public static readonly CVarDef<float> GhostRespawnTime =
        CVarDef.Create("ghost.respawn_time", 5f, CVar.SERVERONLY);

    public static readonly CVarDef<int> GhostRespawnMaxPlayers =
        CVarDef.Create("ghost.respawn_max_players", 40, CVar.SERVERONLY);



    // this also does nothing
    public static readonly CVarDef<bool> GhostAllowSameCharacter =
        CVarDef.Create("ghost.allow_same_character", false, CVar.SERVERONLY);
}
