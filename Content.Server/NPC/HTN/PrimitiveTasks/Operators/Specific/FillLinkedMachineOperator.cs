using Content.Server.Disposal.Unit.Components;
using Content.Shared.Body.Part;
using Content.Shared.DeviceLinking;
using Content.Shared.Disposal;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Materials;
using Content.Shared.Silicons.Bots;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Whitelist;
using Robust.Shared.Containers;


namespace Content.Server.NPC.HTN.PrimitiveTasks.Operators.Specific;

public sealed partial class FillLinkedMachineOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    private SharedMaterialStorageSystem _sharedMaterialStorage = default!;
    private SharedDisposalUnitSystem _sharedDisposalUnitSystem = default!;
    private SharedHandsSystem _sharedHandsSystem = default!;
    private SharedContainerSystem _containerSystem = default!;
    private EntityWhitelistSystem _whitelistSystem = default!;

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
        _containerSystem = sysManager.GetEntitySystem<SharedContainerSystem>();
        _whitelistSystem = sysManager.GetEntitySystem<EntityWhitelistSystem>();
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
            || !_entManager.TryGetComponent(owner, out FillbotComponent? fillbot)
            || !_entManager.HasComponent<HandsComponent>(owner)
            || !_entManager.TryGetComponent(owner, out DeviceLinkSourceComponent? fillbotlinks)
            || fillbotlinks.LinkedPorts.Count != 1
            || fillbot.LinkedSinkEntity == null
            || _entManager.Deleted(fillbot.LinkedSinkEntity))
            return HTNOperatorStatus.Failed;

        _entManager.TryGetComponent(fillbot.LinkedSinkEntity, out MaterialStorageComponent? linkedStorage);
        _entManager.TryGetComponent(fillbot.LinkedSinkEntity, out DisposalUnitComponent? disposalUnit);
        _entManager.TryGetComponent(fillbot.LinkedSinkEntity, out BallisticAmmoProviderComponent? ballisticAmmo);

        var heldItem = _sharedHandsSystem.GetActiveItem(owner);

        if (heldItem == null || _entManager.HasComponent<BodyPartComponent>(heldItem))
        {
            _sharedHandsSystem.TryDrop(owner);
            return HTNOperatorStatus.Failed;
        }

        if (linkedStorage is not null
            && _sharedMaterialStorage.TryInsertMaterialEntity(owner, heldItem.Value, fillbot.LinkedSinkEntity!.Value))
            return HTNOperatorStatus.Finished;

        else if (disposalUnit is not null)
        {
            _sharedDisposalUnitSystem.DoInsertDisposalUnit(fillbot.LinkedSinkEntity!.Value, heldItem.Value, owner);
            return HTNOperatorStatus.Finished;
        }

        else if (ballisticAmmo is not null)
        {
            // Check if we can insert based on capacity and whitelist
            var currentShots = ballisticAmmo.Entities.Count + ballisticAmmo.UnspawnedCount;
            if (currentShots >= ballisticAmmo.Capacity)
            {
                _sharedHandsSystem.TryDrop(owner);
                return HTNOperatorStatus.Failed;
            }

            if (_whitelistSystem.IsWhitelistFailOrNull(ballisticAmmo.Whitelist, heldItem.Value))
            {
                _sharedHandsSystem.TryDrop(owner);
                return HTNOperatorStatus.Failed;
            }

            // Insert the ammunition
            ballisticAmmo.Entities.Add(heldItem.Value);
            _containerSystem.Insert(heldItem.Value, ballisticAmmo.Container);
            ballisticAmmo.Cycled = true;
            return HTNOperatorStatus.Finished;
        }

        _sharedHandsSystem.TryDrop(owner);
        return HTNOperatorStatus.Failed;
    }
}
