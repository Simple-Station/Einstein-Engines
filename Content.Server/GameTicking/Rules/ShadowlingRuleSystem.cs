using Content.Server.Antag;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Mind;
using Content.Server.Roles;
using Content.Shared._EE.Shadowling;
using Content.Shared.Roles;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.GameTicking.Rules;

public sealed class ShadowlingRuleSystem : GameRuleSystem<ShadowlingRuleComponent>
{
    [Dependency] private readonly SharedRoleSystem _role = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly MindSystem _mind = default!;

    public readonly SoundSpecifier BriefingSound = new SoundPathSpecifier("/Audio/_Goobstation/Ambience/Antag/changeling_start.ogg"); // todo: change later

    [ValidatePrototypeId<EntityPrototype>] EntProtoId _mindRole = "MindRoleShadowling";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingRuleComponent, AfterAntagEntitySelectedEvent>(OnSelectAntag);
    }

    private void OnSelectAntag(EntityUid uid, ShadowlingRuleComponent comp, ref AfterAntagEntitySelectedEvent args)
    {
        MakeShadowling(args.EntityUid, comp);
    }

    public bool MakeShadowling(EntityUid target, ShadowlingRuleComponent rule)
    {
        if (!_mind.TryGetMind(target, out var mindId, out var mind))
            return false;

        _role.MindAddRole(mindId, _mindRole.Id, mind, true);

        TryComp<MetaDataComponent>(target, out var metaData);
        if (metaData == null)
            return false;

        var briefing = Loc.GetString("shadowling-role-greeting");
        var briefingShort = Loc.GetString("shadowling-role-greeting-short", ("name", metaData.EntityName ?? "Unknown"));

        _antag.SendBriefing(target, briefing, Color.MediumPurple, BriefingSound);

        if (_role.MindHasRole<ShadowlingComponent>(mindId, out var mr))
            AddComp(mr.Value, new RoleBriefingComponent { Briefing = briefingShort }, overwrite: true);

        EnsureComp<ShadowlingComponent>(target);
        return true;
    }
}
