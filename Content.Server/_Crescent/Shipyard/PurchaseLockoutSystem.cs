using Content.Server.Chat.Systems;
using Content.Server.Shuttles.Components;
using Content.Shared.Access.Components;
using Content.Shared.Chat;
using Content.Shared.Interaction;
using Content.Shared.UserInterface;
using Robust.Shared.Timing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Audio;

namespace Content.Server._Crescent.Shipyard;

public sealed partial class PurchaseLockoutSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PurchaseLockoutComponent, ActivatableUIOpenAttemptEvent>(OnUIOpenAttempt);
        SubscribeLocalEvent<PurchaseLockoutComponent, AfterInteractUsingEvent>(OnAfterInteractUsing);
    }
    private void OnUIOpenAttempt(EntityUid uid, PurchaseLockoutComponent component, ActivatableUIOpenAttemptEvent args)
    {
        var timePassed = _timing.CurTime - component.CreationTime;
        if (timePassed < component.LockoutTime)
        {
            _chat.TrySendInGameICMessage(uid, Loc.GetString("purchase-lockout-message", ("name", component.Purchaser ?? "ERROR"), ("seconds", (int) (component.LockoutTime.TotalSeconds - timePassed.TotalSeconds))), InGameICChatType.Speak, false);
            args.Cancel();
            return;
        }

        RemCompDeferred<PurchaseLockoutComponent>(uid);
    }

    private void OnAfterInteractUsing(EntityUid uid, PurchaseLockoutComponent component, AfterInteractUsingEvent args)
    {
        if (!TryComp<IdCardComponent>(args.Used, out var id))
            return;

        if (id.FullName != component.Purchaser)
        {
            _audio.PlayPvs("/Audio/Machines/custom_deny.ogg", uid, AudioParams.Default);
            return;
        }

        _audio.PlayPvs("/Audio/Machines/high_tech_confirm.ogg", uid, AudioParams.Default);
        RemCompDeferred<PurchaseLockoutComponent>(uid);
        return;
    }
}
