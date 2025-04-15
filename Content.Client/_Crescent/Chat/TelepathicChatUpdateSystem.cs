using Content.Shared.Crescent.Psionics;
using Content.Client.Chat.Managers;
using Robust.Client.Player;

namespace Content.Client.Crescent.Chat
{
    public sealed class TelepathicChatUpdateSystem : EntitySystem
    {
        [Dependency] private readonly IChatManager _chatManager = default!;
        [Dependency] private readonly IPlayerManager _playerManager = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<TelepathicComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<TelepathicComponent, ComponentRemove>(OnRemove);
        }

        public TelepathicComponent? Player => CompOrNull<TelepathicComponent>(_playerManager.LocalPlayer?.ControlledEntity);
        public bool IsTelepathic => Player != null;

        private void OnInit(EntityUid uid, TelepathicComponent component, ComponentInit args)
        {
            _chatManager.UpdatePermissions();
        }

        private void OnRemove(EntityUid uid, TelepathicComponent component, ComponentRemove args)
        {
            _chatManager.UpdatePermissions();
        }
    }
}
