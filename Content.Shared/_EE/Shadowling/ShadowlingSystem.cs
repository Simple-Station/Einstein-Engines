namespace Content.Shared._EE.Shadowling;


/// <summary>
/// This handles...
/// </summary>
public sealed class ShadowlingSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingComponent, ComponentInit>(OnInit);
    }

    public void OnInit(EntityUid uid, ShadowlingComponent comp, ref ComponentInit args)
    {

    }
}
