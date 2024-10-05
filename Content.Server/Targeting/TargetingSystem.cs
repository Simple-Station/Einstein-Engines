using Content.Shared.Targeting;
using Content.Shared.Targeting.Events;
using Robust.Server.Audio;
using Robust.Shared.Audio;

namespace Content.Server.Targeting;
public sealed class TargetingSystem : SharedTargetingSystem
{
    [Dependency] private readonly AudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<TargetChangeEvent>(OnTargetChange);
    }

    private void OnTargetChange(TargetChangeEvent message, EntitySessionEventArgs args)
    {
        if (!TryComp<TargetingComponent>(GetEntity(message.Uid), out var target))
            return;

        // Todo, get a better sound for this shit.
        //_audio.PlayGlobal(target.SwapSound, args.SenderSession, AudioParams.Default.WithVolume(-8f));
        target.Target = message.BodyPart;
        Dirty(GetEntity(message.Uid), target);
    }
}