using System.Linq;
using Content.Shared.EntityEffects;
using Content.Shared.Localizations;
using Robust.Shared.Prototypes;
using Content.Shared.Mind.Components;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;

namespace Content.Server.EntityEffects.EffectConditions;

public sealed partial class JobCondition : EntityEffectCondition
{
    [DataField(required: true)] public List<ProtoId<JobPrototype>> Job;

    public override bool Condition(EntityEffectBaseArgs args)
    {
        args.EntityManager.TryGetComponent<MindContainerComponent>(args.TargetEntity, out var mindContainer);
        if (mindContainer != null && mindContainer.Mind != null)
        {
            var prototypeManager = IoCManager.Resolve<IPrototypeManager>();
            if (args.EntityManager.TryGetComponent<JobComponent>(mindContainer?.Mind, out var comp) && prototypeManager.TryIndex(comp.Prototype, out var prototype))
            {
                foreach (var jobId in Job)
                    if (prototype.ID == jobId)
                        return true;
            }
        }

        return false;
    }

    public override string GuidebookExplanation(IPrototypeManager prototype)
    {
        var localizedNames = Job.Select(jobId => prototype.Index(jobId).LocalizedName).ToList();
        return Loc.GetString("reagent-effect-condition-guidebook-job-condition", ("job", ContentLocalizationManager.FormatListToOr(localizedNames)));
    }
}


