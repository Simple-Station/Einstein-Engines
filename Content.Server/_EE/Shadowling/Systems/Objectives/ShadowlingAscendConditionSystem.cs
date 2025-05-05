using Content.Shared._EE.Shadowling;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;


namespace Content.Server._EE.Shadowling.Objectives;


/// <summary>
/// This handles the Ascension objective
/// </summary>
public sealed class ShadowlingAscendConditionSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<ShadowlingAscendConditionComponent, ObjectiveGetProgressEvent>(OnObjectiveProgress);
        SubscribeLocalEvent<ShadowlingAscendConditionComponent, ObjectiveAfterAssignEvent>(OnAfterAssign);
    }

    private void OnAfterAssign(EntityUid uid, ShadowlingAscendConditionComponent comp, ref ObjectiveAfterAssignEvent args)
    {
        comp.MindId = args.MindId;
    }

    private void OnObjectiveProgress(Entity<ShadowlingAscendConditionComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = GetProgress(args.Mind);
    }

    private float GetProgress(MindComponent mind)
    {
        // idek why this shit is not working i tried everything IM DONE!!!
        if (mind.OwnedEntity == null)
            return 0f;

        if (TryComp<ShadowlingComponent>(mind.OwnedEntity, out var comp))
        {
            if (comp.CurrentPhase == ShadowlingPhases.Ascension)
                return 1f;
        }
        return 0f;
    }
}
