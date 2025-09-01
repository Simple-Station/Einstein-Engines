using System.Linq;
using Content.Shared._Crescent.PvsAutoOverride;
using Content.Shared.Players;
using Robust.Shared.Map;
using Robust.Shared.Player;


namespace Content.Server._Crescent.HullrotPvsAutoOverride;


/// <summary>
/// This handles...
/// </summary>
public sealed class PvsAutoOverrideSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly ISharedPlayerManager _players = default!;
    private Dictionary<IComponent, Func<EntityUid, IComponent, bool>> pvsConditions = new();
    /// <inheritdoc/>
    public override void Initialize()
    {
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        HashSet<EntityUid> relevantEntities = new();
        var playerData = _players.GetAllPlayerData();
        HashSet<EntityUid> validPlayers = new();
        foreach (var player in playerData)
        {
            if (player?.ContentData()?.Mind is null)
                continue;
            validPlayers.Add(player!.ContentData()!.Mind!.Value);

        }
    }
}
