using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Content.Shared.Verbs;
using Robust.Shared.Utility;

namespace Content.Server.DetailExaminable
{
    public sealed class DetailExaminableSystem : EntitySystem
    {
        [Dependency] private readonly ExamineSystemShared _examineSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<DetailExaminableComponent, ExaminedEvent>(OnExamined);
        }

        private void OnExamined(EntityUid uid, DetailExaminableComponent component, ExaminedEvent entity)
        {
            // Only show details if the entity's name matches its metadata name (original logic)
            if (Identity.Name(entity.Examined, EntityManager) != MetaData(entity.Examined).EntityName)
                return;

            // Only show details if the examiner is in details range
            if (!entity.IsInDetailsRange)
                return;

            // Append the detail content directly to the examine message
            entity.PushMarkup(component.Content);
        }
    }
}
