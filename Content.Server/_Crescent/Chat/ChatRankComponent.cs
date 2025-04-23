namespace Content.Server.Crescent.Chat;

/// <summary>
/// Stores "rank" of the user for certain contexts.
/// Mostly given by role.
/// </summary>

[RegisterComponent]
public sealed partial class ChatRankComponent : Component
{
    [DataField]
    public string Rank = "crescent-rank-private";
}
