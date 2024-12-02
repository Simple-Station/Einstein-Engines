#region

using Content.Client.UserInterface.Screens;
using Content.Client.UserInterface.Systems.Gameplay;
using Content.Client.Voting;
using JetBrains.Annotations;
using Robust.Client.UserInterface.Controllers;

#endregion


namespace Content.Client.UserInterface.Systems.Vote;


[UsedImplicitly]
public sealed class VoteUIController : UIController
{
    [Dependency] private readonly IVoteManager _votes = default!;

    public override void Initialize()
    {
        base.Initialize();
        var gameplayStateLoad = UIManager.GetUIController<GameplayStateLoadController>();
        gameplayStateLoad.OnScreenLoad += OnScreenLoad;
        gameplayStateLoad.OnScreenUnload += OnScreenUnload;
    }

    private void OnScreenLoad()
    {
        switch (UIManager.ActiveScreen)
        {
            case SeparatedChatGameScreen separated:
                _votes.SetPopupContainer(separated.VoteMenu);
                break;
            case OverlayChatGameScreen overlay:
                _votes.SetPopupContainer(overlay.VoteMenu);
                break;
        }
    }

    private void OnScreenUnload() => _votes.ClearPopupContainer();
}
