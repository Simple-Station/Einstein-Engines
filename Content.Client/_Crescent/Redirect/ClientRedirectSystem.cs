using Content.Shared.Crescent.Redirect;
using Robust.Client;

namespace Content.Client.Crescent.Redirect;


public sealed class ClientRedirectSystem : EntitySystem
{


    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<RedirectMessage>(OnRedirectMessage);

    }

    private void OnRedirectMessage(RedirectMessage ev)
    {
        /// We're cooking hard here... DLL diggers.. will you be able to figure out
        /// what feature we're adding next ?  , SPCR - 2024
        IoCManager.Resolve<IGameController>().Redial(ev.RedirectUrl, null);
        return;
    }

}
