#region

using Content.Shared.Players;
using Robust.Shared.Player;

#endregion


namespace Content.Client.Players;


public sealed class PlayerSystem : SharedPlayerSystem
{
    public override ContentPlayerData? ContentData(ICommonSession? session) => null;
}
