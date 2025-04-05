using Content.Server.Antag;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.GameTicking.Rules;
using Content.Server.Mind;
using Content.Server.Objectives;
using Content.Server.Roles;
using Content.Shared._EE.Shadowling;
using Content.Shared.NPC.Prototypes;
using Content.Shared.NPC.Systems;
using Content.Shared.Roles;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Content.Shared.Silicon.Components;

namespace Content.Server.GameTicking.Rules;

public sealed class ShadowlingRuleSystem : GameRuleSystem<ShadowlingRuleComponent>
{
    [Dependency] private readonly SharedRoleSystem _role = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly MindSystem _mind = default!;

    public readonly SoundSpecifier BriefingSound = new SoundPathSpecifier("/Audio/_Goobstation/Ambience/Antag/changeling_start.ogg"); // todo: change later

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

        TryComp<MetaDataComponent>(target, out var metaData);
        if (metaData == null)
            return false;

        var briefing = Loc.GetString("shadowling-role-greeting");
        var briefingShort = Loc.GetString("shadowling-role-greeting-short", ("name", metaData.EntityName ?? "Unknown"));

        _antag.SendBriefing(target, briefing, Color.Yellow, BriefingSound);

        if (_role.MindHasRole<ShadowlingComponent>(mindId, out var mr))
            AddComp(mr.Value, new RoleBriefingComponent { Briefing = briefingShort }, overwrite: true);

        _role.MindAddRole(mindId, new EntProtoId("Shadowling"), mind, true);
        return true;
    }
}
