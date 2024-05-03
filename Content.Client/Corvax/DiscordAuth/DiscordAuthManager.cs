using Content.Shared.Corvax.DiscordAuth;
using Robust.Client.State;
using Robust.Shared.Network;

namespace Content.Client.Corvax.DiscordAuth;

public sealed class DiscordAuthManager
{
    [Dependency] private readonly IClientNetManager _net = default!;
    [Dependency] private readonly IStateManager _state = default!;


    public string AuthUrl { get; private set; } = string.Empty;


    public void Initialize()
    {
        _net.RegisterNetMessage<DiscordAuthCheckMessage>();
        _net.RegisterNetMessage<DiscordAuthRequiredMessage>(OnDiscordAuthRequired);
    }


    private void OnDiscordAuthRequired(DiscordAuthRequiredMessage message)
    {
        if (_state.CurrentState is not DiscordAuthState)
        {
            AuthUrl = message.AuthUrl;
            _state.RequestStateChange<DiscordAuthState>();
        }
    }
}
