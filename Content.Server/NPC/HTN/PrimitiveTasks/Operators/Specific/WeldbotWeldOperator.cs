using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Chat.Systems;
using Content.Shared.Chat;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Emag.Components;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Silicons.Bots;
using Content.Shared.Tag;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;


namespace Content.Server.NPC.HTN.PrimitiveTasks.Operators.Specific;

public sealed partial class WeldbotWeldOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    private ChatSystem _chat = default!;
    private WeldbotSystem _weldbot = default!;
    private SharedAudioSystem _audio = default!;
    private SharedInteractionSystem _interaction = default!;
    private SharedPopupSystem _popup = default!;
    private DamageableSystem _damageableSystem = default!;
    private TagSystem _tagSystem = default!;
    private FlammableSystem _flammableSystem = default!;

    /// <summary>
    /// Target entity to inject.
    /// </summary>
    [DataField("targetKey", required: true)]
    public string TargetKey = string.Empty;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _chat = sysManager.GetEntitySystem<ChatSystem>();
        _weldbot = sysManager.GetEntitySystem<WeldbotSystem>();
        _audio = sysManager.GetEntitySystem<SharedAudioSystem>();
        _interaction = sysManager.GetEntitySystem<SharedInteractionSystem>();
        _popup = sysManager.GetEntitySystem<SharedPopupSystem>();
        _damageableSystem = sysManager.GetEntitySystem<DamageableSystem>();
        _tagSystem = sysManager.GetEntitySystem<TagSystem>();
        _flammableSystem = sysManager.GetEntitySystem<FlammableSystem>();
    }

    public override void TaskShutdown(NPCBlackboard blackboard, HTNOperatorStatus status)
    {
        base.TaskShutdown(blackboard, status);
        blackboard.Remove<EntityUid>(TargetKey);
    }

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        if (!blackboard.TryGetValue<EntityUid>(TargetKey, out var target, _entMan) || _entMan.Deleted(target))
            return HTNOperatorStatus.Failed;

        var tagPrototype = _prototypeManager.Index<TagPrototype>("SiliconMob");

        if (_entMan.TryGetComponent<TagComponent>(target, out var tagComponent) && _tagSystem.HasTag(tagComponent, tagPrototype))
            return HTNOperatorStatus.Failed;

        if (!_entMan.TryGetComponent<WeldbotComponent>(owner, out var botComp))
            return HTNOperatorStatus.Failed;

        if (!_entMan.TryGetComponent<DamageableComponent>(target, out var damage))
            return HTNOperatorStatus.Failed;

        if (!_interaction.InRangeUnobstructed(owner, target))
            return HTNOperatorStatus.Failed;

        var total = damage.DamagePerGroup["Brute"].Value;

        if (total == 0 && !_entMan.HasComponent<EmaggedComponent>(owner))
            return HTNOperatorStatus.Failed;

        if (botComp.IsEmagged)
        {
            if (!_entMan.TryGetComponent<FlammableComponent>(target, out var flammableComponent))
                return HTNOperatorStatus.Failed;

            _flammableSystem.Ignite(target, owner, flammableComponent);
        }
        else
        {
            if (!_prototypeManager.TryIndex<DamageGroupPrototype>("Brute", out var prototype))
            {
                return HTNOperatorStatus.Failed;
            }

            _damageableSystem.TryChangeDamage(target, new DamageSpecifier(prototype, -5), true, false, damage);
        }

        _audio.PlayPvs(botComp.WeldSound, target);
        _chat.TrySendInGameICMessage(owner, Loc.GetString("weldbot-finish-weld"), InGameICChatType.Speak, hideChat: true, hideLog: true);
        return HTNOperatorStatus.Finished;
    }
}
