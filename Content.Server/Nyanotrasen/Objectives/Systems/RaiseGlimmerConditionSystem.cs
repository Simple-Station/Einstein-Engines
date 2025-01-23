using Content.Shared.Psionics.Glimmer;
using Content.Server.Objectives.Components;
using Content.Shared.Objectives.Components;

namespace Content.Server.Objectives.Systems
{
    public sealed class RaiseGlimmerConditionSystem : EntitySystem
    {
        [Dependency] private readonly IEntitySystemManager _sysMan = default!;
        [Dependency] private readonly MetaDataSystem _metaData = default!;
        [Dependency] private readonly GlimmerSystem _glimmer = default!;
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<RaiseGlimmerConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
            SubscribeLocalEvent<RaiseGlimmerConditionComponent, ObjectiveAfterAssignEvent>(OnAfterAssign);
        }

        private void OnAfterAssign(EntityUid uid, RaiseGlimmerConditionComponent comp, ref ObjectiveAfterAssignEvent args)
        {
            var title = Loc.GetString("objective-condition-raise-glimmer-title", ("target", comp.Target));
            var description = Loc.GetString("objective-condition-raise-glimmer-description", ("target", comp.Target));

            _metaData.SetEntityName(uid, title, args.Meta);
            _metaData.SetEntityDescription(uid, description, args.Meta);
        }

        private void OnGetProgress(EntityUid uid, RaiseGlimmerConditionComponent comp, ref ObjectiveGetProgressEvent args)
        {
            args.Progress = GetProgress(comp.Target);
        }

        private float GetProgress(float target)
        {
            var progress = Math.Min(_glimmer.GlimmerOutput / target, 1f);
            return progress;
        }
    }
}
