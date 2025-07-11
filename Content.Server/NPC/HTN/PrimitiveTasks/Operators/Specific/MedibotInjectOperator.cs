using Content.Server.Chat.Systems;
using Content.Server.NPC.Components;
using Content.Shared.Chat;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Emag.Components;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Silicon.Components;
using Content.Shared.Silicons.Bots;
using Robust.Shared.Audio.Systems;

namespace Content.Server.NPC.HTN.PrimitiveTasks.Operators.Specific;

public sealed partial class MedibotInjectOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entMan = default!;
    private ChatSystem _chat = default!;
    private MedibotSystem _medibot = default!;
    private SharedAudioSystem _audio = default!;
    private SharedInteractionSystem _interaction = default!;
    private SharedPopupSystem _popup = default!;
    private SharedSolutionContainerSystem _solutionContainer = default!;

    /// <summary>
    /// Target entity to inject.
    /// </summary>
    [DataField("targetKey", required: true)]
    public string TargetKey = string.Empty;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _chat = sysManager.GetEntitySystem<ChatSystem>();
        _medibot = sysManager.GetEntitySystem<MedibotSystem>();
        _audio = sysManager.GetEntitySystem<SharedAudioSystem>();
        _interaction = sysManager.GetEntitySystem<SharedInteractionSystem>();
        _popup = sysManager.GetEntitySystem<SharedPopupSystem>();
        _solutionContainer = sysManager.GetEntitySystem<SharedSolutionContainerSystem>();
    }

    public override void TaskShutdown(NPCBlackboard blackboard, HTNOperatorStatus status)
    {
        base.TaskShutdown(blackboard, status);
        blackboard.Remove<EntityUid>(TargetKey);
    }

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        // TODO: Wat
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        if (!blackboard.TryGetValue<EntityUid>(TargetKey, out var target, _entMan) || _entMan.Deleted(target))
            return HTNOperatorStatus.Failed;

        if (_entMan.HasComponent<SiliconComponent>(target))
            return HTNOperatorStatus.Failed;

        if (!_entMan.TryGetComponent<MedibotComponent>(owner, out var botComp))
            return HTNOperatorStatus.Failed;

        if (!_entMan.TryGetComponent<DamageableComponent>(target, out var damage))
            return HTNOperatorStatus.Failed;

        if (!_solutionContainer.TryGetInjectableSolution(target, out var injectable, out _))
            return HTNOperatorStatus.Failed;

        if (!_interaction.InRangeUnobstructed(owner, target))
            return HTNOperatorStatus.Failed;

        var total = damage.TotalDamage;

        // always inject healthy patients when emagged
        if (total == 0 && !_entMan.HasComponent<EmaggedComponent>(owner))
            return HTNOperatorStatus.Failed;

        if (!_entMan.TryGetComponent<MobStateComponent>(target, out var mobState))
            return HTNOperatorStatus.Failed;

        var state = mobState.CurrentState;
        if (!_medibot.TryGetTreatment(botComp, mobState.CurrentState, out var treatment) || !treatment.IsValid(total))
            return HTNOperatorStatus.Failed;

        _entMan.EnsureComponent<NPCRecentlyInjectedComponent>(target);
        _solutionContainer.TryAddReagent(injectable.Value, treatment.Reagent, treatment.Quantity, out _);
        _popup.PopupEntity(Loc.GetString("hypospray-component-feel-prick-message"), target, target);
        _audio.PlayPvs(botComp.InjectSound, target);
        _chat.TrySendInGameICMessage(owner, Loc.GetString("medibot-finish-inject"), InGameICChatType.Speak, hideChat: true, hideLog: true);
        return HTNOperatorStatus.Finished;
    }
}
