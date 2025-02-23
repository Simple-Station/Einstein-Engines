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

    public const string SiliconTag = "SiliconMob";
    public const string WeldotFixableStructureTag = "WeldbotFixableStructure";

    public const float EmaggedBurnDamage = 10;
    public const float SiliconRepairAmount = 30;
    public const float StructureRepairAmount = 5;

    /// <summary>
    /// Target entity to inject.
    /// </summary>
    [DataField(required: true)]
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

        var tagSiliconMobPrototype = _prototypeManager.Index<TagPrototype>(SiliconTag);
        var tagWeldFixableStructurePrototype = _prototypeManager.Index<TagPrototype>(WeldotFixableStructureTag);

        if(!_entMan.TryGetComponent<TagComponent>(target, out var tagComponent))
            return HTNOperatorStatus.Failed;

        var weldableIsSilicon = _tagSystem.HasTag(tagComponent, tagSiliconMobPrototype);
        var weldableIsStructure = _tagSystem.HasTag(tagComponent, tagWeldFixableStructurePrototype);

        if ((!weldableIsSilicon && !weldableIsStructure)
            || !_entMan.TryGetComponent<WeldbotComponent>(owner, out var botComp)
            || !_entMan.TryGetComponent<DamageableComponent>(target, out var damage)
            || !_interaction.InRangeUnobstructed(owner, target))
            return HTNOperatorStatus.Failed;

        var canWeldSilicon = damage.DamagePerGroup["Brute"].Value > 0  || _entMan.HasComponent<EmaggedComponent>(owner);
        var canWeldStructure = damage.TotalDamage.Value > 0;

        if ((!canWeldSilicon && weldableIsSilicon) || (!canWeldStructure && weldableIsStructure))
            return HTNOperatorStatus.Failed;

        if (botComp.IsEmagged)
        {
            if (!_prototypeManager.TryIndex<DamageGroupPrototype>("Burn", out var prototype) || weldableIsStructure)
                return HTNOperatorStatus.Failed;

            _damageableSystem.TryChangeDamage(target, new DamageSpecifier(prototype, EmaggedBurnDamage), true, false, damage);
        }
        else
        {
            if (weldableIsSilicon)
            {
                if (!_prototypeManager.TryIndex<DamageGroupPrototype>("Brute", out var prototype))
                    return HTNOperatorStatus.Failed;

                _damageableSystem.TryChangeDamage(target, new DamageSpecifier(prototype, -SiliconRepairAmount), true, false, damage);
            }
            else if (weldableIsStructure)
            {
                //If a structure explicitly has a tag to allow a Weldbot to fix it, trust that we can just do so no matter what the damage actually is.
                _damageableSystem.ChangeAllDamage(target, damage, -StructureRepairAmount);
            }
            else
            {
                return HTNOperatorStatus.Failed;
            }
        }

        _audio.PlayPvs(botComp.WeldSound, target);

        if((weldableIsSilicon && damage.DamagePerGroup["Brute"].Value == 0)
            || (weldableIsStructure && damage.TotalDamage.Value == 0)) //only say "all done if we're actually done!"
            _chat.TrySendInGameICMessage(owner, Loc.GetString("weldbot-finish-weld"), InGameICChatType.Speak, hideChat: true, hideLog: true);

        return HTNOperatorStatus.Finished;
    }
}
