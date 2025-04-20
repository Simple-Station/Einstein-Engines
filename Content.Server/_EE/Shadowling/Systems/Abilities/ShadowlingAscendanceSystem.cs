using Content.Shared._EE.Shadowling;


namespace Content.Server._EE.Shadowling;


/// <summary>
/// debug
/// </summary>
public sealed class ShadowlingAscendanceSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingAscendanceComponent, AscendanceEvent>(OnAscendance);
    }

    private void OnAscendance(EntityUid uid, ShadowlingAscendanceComponent component, AscendanceEvent args)
    {
        RaiseLocalEvent(uid, new PhaseChangedEvent(ShadowlingPhases.Ascension));
    }

}
