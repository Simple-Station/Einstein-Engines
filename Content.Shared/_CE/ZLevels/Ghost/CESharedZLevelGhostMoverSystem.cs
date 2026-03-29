/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */


using Content.Shared._CE.ZLevels.Core.EntitySystems;

namespace Content.Shared._CE.ZLevels.Ghost;

public abstract class CESharedZLevelGhostMoverSystem : EntitySystem
{
    [Dependency] private readonly CESharedZLevelsSystem _zLevel = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CEZLevelGhostMoverComponent, CEZLevelActionUp>(OnZLevelUp);
        SubscribeLocalEvent<CEZLevelGhostMoverComponent, CEZLevelActionDown>(OnZLevelDown);
    }

    private void OnZLevelDown(Entity<CEZLevelGhostMoverComponent> ent, ref CEZLevelActionDown args)
    {
        if (args.Handled)
            return;

        args.Handled = _zLevel.TryMoveDown(ent);
    }

    private void OnZLevelUp(Entity<CEZLevelGhostMoverComponent> ent, ref CEZLevelActionUp args)
    {
        if (args.Handled)
            return;

        args.Handled = _zLevel.TryMoveUp(ent);
    }
}
