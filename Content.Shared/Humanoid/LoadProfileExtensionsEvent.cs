using Content.Shared.Preferences;
using Robust.Shared.Player;

namespace Content.Shared.Humanoid;

public sealed partial class LoadProfileExtensionsEvent : EntityEventArgs
{
    public EntityUid Mob { get; }
    public ICommonSession Player { get; }
    public string? JobId { get; }
    public HumanoidCharacterProfile Profile { get; }
    public bool GenerateLoadouts { get; }

    public LoadProfileExtensionsEvent(EntityUid mob,
        ICommonSession player,
        string? jobId,
        HumanoidCharacterProfile profile,
        bool generateLoadouts)
    {
        Mob = mob;
        Player = player;
        JobId = jobId;
        Profile = profile;
        GenerateLoadouts = generateLoadouts;
    }
}
