<<<<<<< HEAD:Content.Server/NPC/HTN/PrimitiveTasks/Operators/Specific/BlobPodZombifyOperator.cs
using Content.Server.Blob.NPC.BlobPod;
||||||| parent of b1f1be5a79 (Cleanup (#593)):Content.Server/NPC/HTN/PrimitiveTasks/Operators/Specific/BlobPodZombifyOperator.cs
using Content.Server.Backmen.Blob.NPC.BlobPod;
using Content.Shared.Backmen.Blob.Components;
using Content.Shared.Backmen.Blob.NPC.BlobPod;
=======
using Content.Server.Backmen.Blob.NPC.BlobPod;
using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.HTN.PrimitiveTasks;
using Content.Shared.Backmen.Blob.Components;
using Content.Shared.Backmen.Blob.NPC.BlobPod;
>>>>>>> b1f1be5a79 (Cleanup (#593)):Content.Server/Backmen/NPC/HTN/PrimitiveTasks/Operators/Specific/BlobPodZombifyOperator.cs

namespace Content.Server.Backmen.NPC.HTN.PrimitiveTasks.Operators.Specific;

public sealed class BlobPodZombifyOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    private BlobPodSystem _blobPodSystem = default!;

    [DataField("zombifyKey")]
    public string ZombifyKey = string.Empty;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _blobPodSystem = sysManager.GetEntitySystem<BlobPodSystem>();
    }

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);
        var target = blackboard.GetValue<EntityUid>(ZombifyKey);

        if (!target.IsValid() || _entManager.Deleted(target))
            return HTNOperatorStatus.Failed;

        if (!_entManager.TryGetComponent<BlobPodComponent>(owner, out var pod))
            return HTNOperatorStatus.Failed;

        if (pod.ZombifiedEntityUid != null)
            return HTNOperatorStatus.Continuing;

        if (pod.IsZombifying)
            return HTNOperatorStatus.Continuing;

        if (pod.ZombifyTarget == null)
        {
            if (_blobPodSystem.NpcStartZombify(owner, target, pod))
                return HTNOperatorStatus.Continuing;
            else
                return HTNOperatorStatus.Failed;
        }

        pod.ZombifyTarget = null;
        return HTNOperatorStatus.Finished;
    }
}
