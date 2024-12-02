#region

using Robust.Client.UserInterface.Controllers;

#endregion


namespace Content.Client.UserInterface.Systems.Gameplay;


public sealed class GameplayStateLoadController : UIController
{
    public Action? OnScreenLoad;
    public Action? OnScreenUnload;

    public void UnloadScreen() => OnScreenUnload?.Invoke();

    public void LoadScreen() => OnScreenLoad?.Invoke();
}
