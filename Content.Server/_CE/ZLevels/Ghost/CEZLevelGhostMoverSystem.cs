/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Content.Shared._CE.ZLevels.Ghost;
using Content.Shared.Actions;

namespace Content.Server._CE.ZLevels.Ghost;

public sealed class CEZLevelGhostMoverSystem : CESharedZLevelGhostMoverSystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CEZLevelGhostMoverComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<CEZLevelGhostMoverComponent, ComponentRemove>(OnRemove);
    }

    private void OnMapInit(Entity<CEZLevelGhostMoverComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent, ref ent.Comp.ZLevelUpActionEntity, ent.Comp.UpActionProto);
        _actions.AddAction(ent, ref ent.Comp.ZLevelDownActionEntity, ent.Comp.DownActionProto);
    }

    private void OnRemove(Entity<CEZLevelGhostMoverComponent> ent, ref ComponentRemove args)
    {
        _actions.RemoveAction(ent.Comp.ZLevelUpActionEntity);
        _actions.RemoveAction(ent.Comp.ZLevelDownActionEntity);
    }
}
