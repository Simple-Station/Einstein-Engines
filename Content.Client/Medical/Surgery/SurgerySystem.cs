using Content.Shared.Medical.Surgery;

namespace Content.Client.Medical.Surgery;

public sealed class SurgerySystem : SharedSurgerySystem
{
    public event Action? OnRefresh;

    public override void Update(float frameTime)
    {
        OnRefresh?.Invoke();
    }
}