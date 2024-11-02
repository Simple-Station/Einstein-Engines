using Content.Shared.Medical.Surgery;

namespace Content.Client.Medical.Surgery;

public sealed class SurgerySystem : SharedSurgerySystem
{
    public event Action? OnStep;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<SurgeryUiRefreshEvent>(OnRefresh);
    }

    private void OnRefresh(SurgeryUiRefreshEvent ev)
    {
        OnStep?.Invoke();
    }
}
