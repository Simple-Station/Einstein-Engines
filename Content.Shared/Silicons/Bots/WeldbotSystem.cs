using Content.Shared.Emag.Systems;
using Robust.Shared.Audio.Systems;


namespace Content.Shared.Silicons.Bots;

/// <summary>
/// Handles emagging Weldbots and provides api.
/// </summary>
public sealed class WeldbotSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EmaggableWeldbotComponent, GotEmaggedEvent>(OnEmagged);
    }

    private void OnEmagged(EntityUid uid, EmaggableWeldbotComponent comp, ref GotEmaggedEvent args)
    {
        if (!TryComp<WeldbotComponent>(uid, out var Weldbot))
            return;

        _audio.PlayPredicted(comp.SparkSound, uid, args.UserUid);

        Weldbot.IsEmagged = true;
        args.Handled = true;
    }
}
