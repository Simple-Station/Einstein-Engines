using Robust.Shared.GameObjects;
using Robust.Shared.IoC;

namespace Content.Shared.Key
{
    public sealed class KeySystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<KeyRequiredComponent, ComponentStartup>(OnKeyRequiredStartup);
        }

        private void OnKeyRequiredStartup(EntityUid uid, KeyRequiredComponent component, ComponentStartup args)
        {
            // Logic to handle key requirements
        }

        public bool HasValidKey(EntityUid uid, string requiredKeyId)
        {
            if (EntityManager.TryGetComponent(uid, out KeyComponent? keyComp))
            {
                return keyComp.KeyId == requiredKeyId;
            }
            return false;
        }
    }
}
