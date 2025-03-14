using Content.Server.Chat.Systems;
using Content.Server.Disposal.Unit.Components;
using Content.Shared.Chat;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.DeviceLinking;
using Content.Shared.Disposal;
using Content.Shared.Emag.Components;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Materials;
using Content.Shared.Popups;
using Content.Shared.Silicons.Bots;
using Content.Shared.Tag;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server.NPC.HTN.PrimitiveTasks.Operators.Specific;

public sealed partial class FillLinkedMachineOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    private SharedMaterialStorageSystem _sharedMaterialStorage = default!;
    private SharedDisposalUnitSystem _sharedDisposalUnitSystem = default!;
    private SharedHandsSystem _sharedHandsSystem = default!;

    /// <summary>
    /// Target entity to inject.
    /// </summary>
    [DataField(required: true)]
    public string TargetKey = string.Empty;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _sharedMaterialStorage = sysManager.GetEntitySystem<SharedMaterialStorageSystem>();
        _sharedDisposalUnitSystem = sysManager.GetEntitySystem<SharedDisposalUnitSystem>();
        _sharedHandsSystem = sysManager.GetEntitySystem<SharedHandsSystem>();
    }

    public override void TaskShutdown(NPCBlackboard blackboard, HTNOperatorStatus status)
    {
        base.TaskShutdown(blackboard, status);
        blackboard.Remove<EntityUid>(TargetKey);
    }

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        if (!blackboard.TryGetValue<EntityUid>(TargetKey, out var target, _entManager) || _entManager.Deleted(target)
            || !_entManager.TryGetComponent<FillbotComponent>(owner, out var fillbot)
            ||   !_entManager.TryGetComponent<HandsComponent>(owner, out var fillbotHand)
            || !_entManager.TryGetComponent<DeviceLinkSourceComponent>(owner, out var fillbotlinks)
            || fillbotlinks.LinkedPorts.Count != 1)
            return HTNOperatorStatus.Failed;

        var isMaterialStorage = _entManager.TryGetComponent<MaterialStorageComponent>(
            fillbot.LinkedSinkEntity,
            out var linkedStorage);

        var isDisposalUnit = _entManager.TryGetComponent<DisposalUnitComponent>(
            fillbot.LinkedSinkEntity,
            out var disposalUnit);

        var heldItem = _sharedHandsSystem.GetActiveItem(owner);

        if (heldItem == null)
            return HTNOperatorStatus.Failed;

        if (isMaterialStorage && linkedStorage != null)
        {
            if(_sharedMaterialStorage.TryInsertMaterialEntity(fillbot.Owner, heldItem.Value, linkedStorage.Owner))
                return HTNOperatorStatus.Finished;
        }
        else if (isDisposalUnit && disposalUnit != null)
        {
            _sharedDisposalUnitSystem.DoInsertDisposalUnit(disposalUnit.Owner, heldItem.Value, fillbot.Owner);
            return HTNOperatorStatus.Finished;
        }

        return HTNOperatorStatus.Failed;
    }
}
