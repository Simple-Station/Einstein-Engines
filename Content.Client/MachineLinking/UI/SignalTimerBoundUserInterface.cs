#region

using Content.Shared.MachineLinking;
using Robust.Shared.Timing;

#endregion


namespace Content.Client.MachineLinking.UI;


public sealed class SignalTimerBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    [ViewVariables]
    private SignalTimerWindow? _window;

    public SignalTimerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey) { }

    protected override void Open()
    {
        base.Open();

        _window = new(this);

        if (State != null)
            UpdateState(State);

        _window.OpenCentered();
        _window.OnClose += Close;
        _window.OnCurrentTextChanged += OnTextChanged;
        _window.OnCurrentDelayMinutesChanged += OnDelayChanged;
        _window.OnCurrentDelaySecondsChanged += OnDelayChanged;
    }

    public void OnStartTimer() => SendMessage(new SignalTimerStartMessage());

    private void OnTextChanged(string newText) => SendMessage(new SignalTimerTextChangedMessage(newText));

    private void OnDelayChanged(string newDelay)
    {
        if (_window == null)
            return;
        SendMessage(new SignalTimerDelayChangedMessage(_window.GetDelay()));
    }

    public TimeSpan GetCurrentTime() => _gameTiming.CurTime;

    /// <summary>
    ///     Update the UI state based on server-sent info
    /// </summary>
    /// <param name="state"></param>
    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (_window == null || state is not SignalTimerBoundUserInterfaceState cast)
            return;

        _window.SetCurrentText(cast.CurrentText);
        _window.SetCurrentDelayMinutes(cast.CurrentDelayMinutes);
        _window.SetCurrentDelaySeconds(cast.CurrentDelaySeconds);
        _window.SetShowText(cast.ShowText);
        _window.SetTriggerTime(cast.TriggerTime);
        _window.SetTimerStarted(cast.TimerStarted);
        _window.SetHasAccess(cast.HasAccess);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;
        _window?.Dispose();
    }
}
