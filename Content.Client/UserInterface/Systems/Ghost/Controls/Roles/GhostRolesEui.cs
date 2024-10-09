using System.Linq;
using Content.Client.Eui;
using Content.Client.Lobby;
using Content.Client.Players.PlayTimeTracking;
using Content.Shared.Clothing.Loadouts.Prototypes;
using Content.Shared.Customization.Systems;
using Content.Shared.Eui;
using Content.Shared.Ghost.Roles;
using Content.Shared.Preferences;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;

namespace Content.Client.UserInterface.Systems.Ghost.Controls.Roles
{
    [UsedImplicitly]
    public sealed class GhostRolesEui : BaseEui
    {
        private readonly GhostRolesWindow _window;
        private GhostRoleRulesWindow? _windowRules = null;
        private uint _windowRulesId = 0;

        public GhostRolesEui()
        {
            _window = new GhostRolesWindow();

            _window.OnRoleRequestButtonClicked += info =>
            {
                _windowRules?.Close();

                if (info.Kind == GhostRoleKind.RaffleJoined)
                {
                    SendMessage(new LeaveGhostRoleRaffleMessage(info.Identifier));
                    return;
                }

                _windowRules = new GhostRoleRulesWindow(info.Rules, _ =>
                {
                    SendMessage(new RequestGhostRoleMessage(info.Identifier));

                    // if raffle role, close rules window on request, otherwise do
                    // old behavior of waiting for the server to close it
                    if (info.Kind != GhostRoleKind.FirstComeFirstServe)
                        _windowRules?.Close();
                });
                _windowRulesId = info.Identifier;
                _windowRules.OnClose += () =>
                {
                    _windowRules = null;
                };
                _windowRules.OpenCentered();
            };

            _window.OnRoleFollow += info =>
            {
                SendMessage(new FollowGhostRoleMessage(info.Identifier));
            };

            _window.OnClose += () =>
            {
                SendMessage(new CloseEuiMessage());
            };
        }

        public override void Opened()
        {
            base.Opened();
            _window.OpenCentered();
        }

        public override void Closed()
        {
            base.Closed();
            _window.Close();
            _windowRules?.Close();
        }

        public override void HandleState(EuiStateBase state)
        {
            base.HandleState(state);

            if (state is not GhostRolesEuiState ghostState)
                return;
            _window.ClearEntries();

            var entityManager = IoCManager.Resolve<IEntityManager>();
            var sysManager = entityManager.EntitySysManager;
            var spriteSystem = sysManager.GetEntitySystem<SpriteSystem>();
            var requirementsManager = IoCManager.Resolve<JobRequirementsManager>();
            var characterReqs = entityManager.System<CharacterRequirementsSystem>();
            var prefs = IoCManager.Resolve<IClientPreferencesManager>();
            var protoMan = IoCManager.Resolve<IPrototypeManager>();
            var configManager = IoCManager.Resolve<IConfigurationManager>();

            var groupedRoles = ghostState.GhostRoles.GroupBy(
                role => (role.Name, role.Description, role.Requirements));
            foreach (var group in groupedRoles)
            {
                var name = group.Key.Name;
                var description = group.Key.Description;
                // ReSharper disable once ReplaceWithSingleAssignment.True
                var hasAccess = true;

                if (!characterReqs.CheckRequirementsValid(
                    group.Key.Requirements ?? new(),
                    new(),
                    (HumanoidCharacterProfile) (prefs.Preferences?.SelectedCharacter ?? HumanoidCharacterProfile.DefaultWithSpecies()),
                    requirementsManager.GetRawPlayTimeTrackers(),
                    requirementsManager.IsWhitelisted(),
                    new LoadoutPrototype(), // idk
                    entityManager,
                    protoMan,
                    configManager,
                    out var reasons))
                    hasAccess = false;

                _window.AddEntry(name, description, hasAccess, characterReqs.GetRequirementsText(reasons), group, spriteSystem);
            }

            var closeRulesWindow = ghostState.GhostRoles.All(role => role.Identifier != _windowRulesId);
            if (closeRulesWindow)
            {
                _windowRules?.Close();
            }
        }
    }
}
