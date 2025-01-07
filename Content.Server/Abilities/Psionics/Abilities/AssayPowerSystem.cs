using Content.Server.Chat.Managers;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Actions.Events;
using Content.Shared.Chat;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Content.Shared.Psionics.Events;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Server.Player;
using Robust.Shared.Timing;
using Robust.Shared.Player;

namespace Content.Server.Abilities.Psionics;

public sealed class AssayPowerSystem : EntitySystem
{
    [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly AudioSystem _audioSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popups = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PsionicComponent, AssayPowerActionEvent>(OnPowerUsed);
        SubscribeLocalEvent<PsionicComponent, AssayDoAfterEvent>(OnDoAfter);
    }

    private void OnPowerUsed(EntityUid uid, PsionicComponent psionic, AssayPowerActionEvent args)
    {
        if (!_psionics.OnAttemptPowerUse(args.Performer, "assay")
            || psionic.DoAfter is not null)
            return;

        var ev = new AssayDoAfterEvent(_gameTiming.CurTime, args.FontSize, args.FontColor);
        _doAfterSystem.TryStartDoAfter(new DoAfterArgs(EntityManager, args.Performer, args.UseDelay - TimeSpan.FromSeconds(psionic.CurrentAmplification), ev, args.Performer, args.Target, args.Performer)
        {
            BlockDuplicate = true,
            BreakOnMove = true,
            BreakOnDamage = true,
        }, out var doAfterId);
        psionic.DoAfter = doAfterId;

        _popups.PopupEntity(Loc.GetString(args.PopupTarget, ("entity", args.Target)), args.Performer, PopupType.Medium);

        _audioSystem.PlayPvs(args.SoundUse, args.Performer, AudioParams.Default.WithVolume(8f).WithMaxDistance(1.5f).WithRolloffFactor(3.5f));
        _psionics.LogPowerUsed(args.Performer, args.PowerName, args.MinGlimmer, args.MaxGlimmer);
        args.Handled = true;
    }

    private void OnDoAfter(EntityUid uid, PsionicComponent userPsionic, AssayDoAfterEvent args)
    {
        if (userPsionic is null)
            return;
        userPsionic.DoAfter = null;

        var user = uid;
        var target = args.Target;
        if (target == null || args.Cancelled
            || !_playerManager.TryGetSessionByEntity(user, out var session))
            return;

        if (target == user)
        {
            var userAmp = MathF.Round(userPsionic.CurrentAmplification, 2);
            var userDamp = MathF.Round(userPsionic.CurrentDampening, 2);
            var userPotentia = MathF.Round(userPsionic.Potentia, 2);
            var userFeedback = $"[font size={args.FontSize}][color={args.FontColor}]{Loc.GetString("assay-body", ("entity", target), ("amplification", userAmp), ("dampening", userDamp), ("potentia", userPotentia))}[/color][/font]";
            SendDescToChat(userFeedback, session);

            var assaySelf = Loc.GetString("assay-self", ("entity", target));
            _popups.PopupEntity(assaySelf, user, user, PopupType.LargeCaution);

            var assaySelfFeedback = $"[font size={args.FontSize}][color={args.FontColor}]{assaySelf}[/color][/font]";
            SendDescToChat(assaySelfFeedback, session);
            return;
        }

        if (!TryComp<PsionicComponent>(target, out var targetPsionic))
        {
            var noPowers = Loc.GetString("no-powers", ("entity", target));
            _popups.PopupEntity(noPowers, user, user, PopupType.LargeCaution);

            var noPowersFeedback = $"[font size={args.FontSize}][color={args.FontColor}]{noPowers}[/color][/font]";
            SendDescToChat(noPowersFeedback, session);
            return;
        }

        var targetAmp = MathF.Round(targetPsionic.CurrentAmplification, 2);
        var targetDamp = MathF.Round(targetPsionic.CurrentDampening, 2);
        var targetPotentia = MathF.Round(targetPsionic.Potentia, 2);
        var message = $"[font size={args.FontSize}][color={args.FontColor}]{Loc.GetString("assay-body", ("entity", target), ("amplification", targetAmp), ("dampening", targetDamp), ("potentia", targetPotentia))}[/color][/font]";
        SendDescToChat(message, session);

        foreach (var feedback in targetPsionic.AssayFeedback)
        {
            var feedbackMessage = $"[font size={args.FontSize}][color={args.FontColor}]{Loc.GetString(feedback, ("entity", target))}[/color][/font]";
            SendDescToChat(feedbackMessage, session);
        }
    }

    private void SendDescToChat(string feedbackMessage, ICommonSession session)
    {
        _chatManager.ChatMessageToOne(
            ChatChannel.Emotes,
            feedbackMessage,
            feedbackMessage,
            EntityUid.Invalid,
            false,
            session.Channel);
    }
}
