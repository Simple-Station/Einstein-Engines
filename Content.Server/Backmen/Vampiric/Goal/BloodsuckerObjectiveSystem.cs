using Content.Server.Backmen.Vampiric.Role;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Mind;
using Content.Shared.Backmen.Vampiric;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Server.Backmen.Vampiric.Goal;

public sealed class BloodsuckerObjectiveSystem  : GameRuleSystem<BloodsuckerObjectiveComponent>
{
    [Dependency] private readonly MindSystem _mindSystem = default!;
    private ISawmill _sawmill = default!;

    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly IRobustRandom _random = default!;


    public override void Initialize()
    {
        base.Initialize();

        _sawmill = Logger.GetSawmill("preset");

        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndText);

        SubscribeLocalEvent<BloodsuckerConvertConditionComponent, ObjectiveGetProgressEvent>(OnGetConvertProgress);
        SubscribeLocalEvent<BloodsuckerDrinkConditionComponent, ObjectiveGetProgressEvent>(OnGetDrinkProgress);
        SubscribeLocalEvent<BloodsuckerConvertConditionComponent, ObjectiveAssignedEvent>(OnConvertAssigned);
        SubscribeLocalEvent<BloodsuckerConvertConditionComponent, ObjectiveAfterAssignEvent>(OnConvertAfterAssigned);
        SubscribeLocalEvent<BloodsuckerDrinkConditionComponent, ObjectiveAssignedEvent>(OnDrinkAssigned);
        SubscribeLocalEvent<BloodsuckerDrinkConditionComponent, ObjectiveAfterAssignEvent>(OnDrinkAfterAssigned);
    }

    private void OnRoundEndText(RoundEndTextAppendEvent ev)
    {
        var skip = new List<EntityUid>();
        var query = AllEntityQuery<BloodsuckerRuleComponent>();
        while (query.MoveNext(out var vampRule))
        {
            if(vampRule.Elders.Count == 0)
                break;
            ev.AddLine(Loc.GetString("vampire-elder"));

            foreach (var player in vampRule.Elders)
            {
                skip.Add(player.Value);
                var role = CompOrNull<VampireRoleComponent>(player.Value);
                var count = role?.Converted ?? 0;
                var blood = role?.Drink ?? 0;
                var countGoal = 0;
                var bloodGoal = 0f;

                var mind = CompOrNull<MindComponent>(player.Value);
                if (_mindSystem.TryGetObjectiveComp<BloodsuckerDrinkConditionComponent>(player.Value, out var obj1, mind))
                {
                    bloodGoal = obj1.Goal;
                }
                if (_mindSystem.TryGetObjectiveComp<BloodsuckerConvertConditionComponent>(player.Value, out var obj2, mind))
                {
                    countGoal = obj2.Goal;
                }

                _mindSystem.TryGetSession(player.Value, out var session);
                var username = session?.Name;
                if (username != null)
                {
                    ev.AddLine(Loc.GetString("endgame-vamp-name-user", ("name", player.Key), ("username", username)));
                }
                else
                {
                    ev.AddLine(Loc.GetString("endgame-vamp-name", ("name", player.Key)));
                }
                ev.AddLine(Loc.GetString("endgame-vamp-conv",
                    ("count", count), ("goal", countGoal)));
                ev.AddLine(Loc.GetString("endgame-vamp-drink",
                    ("count", blood), ("goal", bloodGoal)));
            }

            ev.AddLine("");
        }

        ev.AddLine(Loc.GetString("vampire-bitten"));
        var q = EntityQueryEnumerator<MindComponent,VampireRoleComponent>();
        while (q.MoveNext(out var mindId,out var mind, out var role))
        {
            if (skip.Contains(mindId))
            {
                continue;
            }
            var count = role?.Converted ?? 0;
            var blood = role?.Drink ?? 0;
            var countGoal = 0;
            var bloodGoal = 0f;

            if (_mindSystem.TryGetObjectiveComp<BloodsuckerDrinkConditionComponent>(mindId, out var obj1,mind))
            {
                bloodGoal = obj1.Goal;
            }
            if (_mindSystem.TryGetObjectiveComp<BloodsuckerConvertConditionComponent>(mindId, out var obj2,mind))
            {
                countGoal = obj2.Goal;
            }

            _mindSystem.TryGetSession(mindId, out var session);
            var username = session?.Name;
            if (username != null)
            {
                ev.AddLine(Loc.GetString("endgame-vamp-name-user", ("name", mind.CharacterName ?? "-"), ("username", username)));
            }
            else
            {
                ev.AddLine(Loc.GetString("endgame-vamp-name", ("name", mind.CharacterName ?? "-")));
            }
            ev.AddLine(Loc.GetString("endgame-vamp-conv",
                ("count", count), ("goal", countGoal)));
            ev.AddLine(Loc.GetString("endgame-vamp-drink",
                ("count", blood), ("goal", bloodGoal)));
        }
    }


    private void OnDrinkAfterAssigned(Entity<BloodsuckerDrinkConditionComponent> condition, ref ObjectiveAfterAssignEvent args)
    {
        _metaData.SetEntityName(condition.Owner, Loc.GetString(condition.Comp.ObjectiveText, ("goal", condition.Comp.Goal)), args.Meta);
        _metaData.SetEntityDescription(condition.Owner, Loc.GetString(condition.Comp.DescriptionText, ("goal", condition.Comp.Goal)), args.Meta);
    }

    private void OnConvertAfterAssigned(Entity<BloodsuckerConvertConditionComponent> condition, ref ObjectiveAfterAssignEvent args)
    {
        _metaData.SetEntityName(condition.Owner, Loc.GetString(condition.Comp.ObjectiveText, ("goal", condition.Comp.Goal)), args.Meta);
        _metaData.SetEntityDescription(condition.Owner, Loc.GetString(condition.Comp.DescriptionText, ("goal", condition.Comp.Goal)), args.Meta);
    }

    private void OnConvertAssigned(Entity<BloodsuckerConvertConditionComponent> ent, ref ObjectiveAssignedEvent args)
    {
        if (args.Mind.OwnedEntity == null)
        {
            args.Cancelled = true;
            return;
        }

        var user = args.Mind.OwnedEntity.Value;
        if (!TryComp<BkmVampireComponent>(user, out var vmp))
        {
            args.Cancelled = true;
            return;
        }

        ent.Comp.Goal = _random.Next(
            1,
            Math.Max(1, // min 1 of 1
                Math.Min(
                    ent.Comp.MaxGoal, // 5
                    (int)Math.Ceiling(Math.Max(_playerManager.PlayerCount, 1f) / ent.Comp.PerPlayers) // per players with max
                    )
                )
            );
    }

    private void OnGetConvertProgress(Entity<BloodsuckerConvertConditionComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        if (args.Mind.OwnedEntity == null || !TryComp<VampireRoleComponent>(args.MindId, out var vmp))
        {
            args.Progress = 0;
            return;
        }

        args.Progress = vmp.Converted / ent.Comp.Goal;
    }

    private void OnDrinkAssigned(Entity<BloodsuckerDrinkConditionComponent> ent, ref ObjectiveAssignedEvent args)
    {
        if (args.Mind.OwnedEntity == null)
        {
            args.Cancelled = true;
            return;
        }

        var user = args.Mind.OwnedEntity.Value;
        if (!TryComp<BkmVampireComponent>(user, out var vmp))
        {
            args.Cancelled = true;
            return;
        }

        ent.Comp.Goal = _random.Next(
            ent.Comp.MinGoal,
            Math.Max(ent.Comp.MinGoal + 1, // min 1 of 1
                ent.Comp.MaxGoal
            )
        );
    }

    private void OnGetDrinkProgress(Entity<BloodsuckerDrinkConditionComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        if (args.Mind.OwnedEntity == null || !TryComp<VampireRoleComponent>(args.MindId, out var vmp))
        {
            args.Progress = 0;
            return;
        }

        args.Progress = vmp.Drink / ent.Comp.Goal;
    }
}
