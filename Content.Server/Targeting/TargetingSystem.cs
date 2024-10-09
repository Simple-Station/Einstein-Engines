using Content.Shared.Body.Systems;
using Content.Shared.Mobs;
using Content.Shared.Targeting;
using Content.Shared.Targeting.Events;
using Robust.Server.Audio;
using Robust.Shared.Audio;

namespace Content.Server.Targeting;
public sealed class TargetingSystem : SharedTargetingSystem
{
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<TargetChangeEvent>(OnTargetChange);
        SubscribeLocalEvent<TargetingComponent, MobStateChangedEvent>(OnMobStateChange);
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

    private void OnMobStateChange(EntityUid uid, TargetingComponent component, MobStateChangedEvent args)
    {
        // Revival is handled by the server, so we're keeping all of this here.
        var changed = false;

        if (args.NewMobState == MobState.Dead)
        {
            foreach (TargetBodyPart part in Enum.GetValues(typeof(TargetBodyPart)))
            {
                component.BodyStatus[part] = TargetIntegrity.Dead;
                changed = true;
            }
        }
        else if (args.OldMobState == MobState.Dead && (args.NewMobState == MobState.Alive || args.NewMobState == MobState.Critical))
        {
            component.BodyStatus = _bodySystem.GetBodyPartStatus(uid);
            changed = true;
        }

        if (changed)
        {
            Dirty(uid, component);
            RaiseNetworkEvent(new TargetIntegrityChangeEvent(GetNetEntity(uid)), uid);
        }
    }
}