using Content.Client.Hands.Systems;
using Content.Client.NPC.HTN;
using Content.Shared._White.CCVar;
using Content.Shared.CCVar;
using Content.Shared.CombatMode;
using Robust.Client.Audio;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Shared.Audio;
using Robust.Shared.Configuration;
using Robust.Shared.Player;

namespace Content.Client.CombatMode;

public sealed class CombatModeSystem : SharedCombatModeSystem
{
    [Dependency] private readonly IOverlayManager _overlayManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;
    [Dependency] private readonly IEyeManager _eye = default!;
    [Dependency] private readonly AudioSystem _audio = default!;

    /// <summary>
    /// Raised whenever combat mode changes.
    /// </summary>
    public event Action<bool>? LocalPlayerCombatModeUpdated;
    public event Action<CombatModeComponent>? LocalPlayerCombatModeAdded;
    public event Action? LocalPlayerCombatModeRemoved;

    private readonly SoundSpecifier _combatModeToggleOnSound = new SoundPathSpecifier("/Audio/_White/Misc/CombatMode/combat_mode_on.ogg");
    private readonly SoundSpecifier _combatModeToggleOffSound = new SoundPathSpecifier("/Audio/_White/Misc/CombatMode/combat_mode_off.ogg");

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CombatModeComponent, AfterAutoHandleStateEvent>(OnHandleState);
        SubscribeLocalEvent<CombatModeComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<CombatModeComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);
        SubscribeLocalEvent<CombatModeComponent, ComponentStartup>(OnStartup);

        Subs.CVar(_cfg, CCVars.CombatModeIndicatorsPointShow, OnShowCombatIndicatorsChanged, true);
    }

    private void OnHandleState(EntityUid uid, CombatModeComponent component, ref AfterAutoHandleStateEvent args)
    {
        UpdateHud(uid);
    }

    private void OnPlayerAttached(EntityUid uid, CombatModeComponent component, LocalPlayerAttachedEvent args)
    {
        LocalPlayerCombatModeAdded?.Invoke(component);
    }

    private void OnPlayerDetached(EntityUid uid, CombatModeComponent component, LocalPlayerDetachedEvent args)
    {
        LocalPlayerCombatModeRemoved?.Invoke();
    }

    private void OnStartup(EntityUid uid, CombatModeComponent component, ComponentStartup args)
    {
        if (_playerManager.LocalEntity == uid)
            LocalPlayerCombatModeAdded?.Invoke(component);
    }

    protected override void OnShutdown(EntityUid uid, CombatModeComponent component, ComponentShutdown args)
    {
        base.OnShutdown(uid, component, args);

        if (_playerManager.LocalEntity == uid)
            LocalPlayerCombatModeRemoved?.Invoke();
    }

    public override void Shutdown()
    {
        _overlayManager.RemoveOverlay<CombatModeIndicatorsOverlay>();

        base.Shutdown();
    }

    public bool IsInCombatMode()
    {
        var entity = _playerManager.LocalEntity;

        if (entity == null)
            return false;

        return IsInCombatMode(entity.Value);
    }

    public override void SetInCombatMode(EntityUid entity, bool value, CombatModeComponent? component = null, bool silent = true)
    {
        base.SetInCombatMode(entity, value, component, silent);
        UpdateHud(entity);

        if (silent || !_cfg.GetCVar(WhiteCVars.CombatModeSoundEnabled))
            return;

        var soundToPlay = value
            ? _combatModeToggleOnSound
            : _combatModeToggleOffSound;
        _audio.PlayLocal(soundToPlay, entity, entity);
    }

    protected override bool IsNpc(EntityUid uid)
    {
        return HasComp<HTNComponent>(uid);
    }

    private void UpdateHud(EntityUid entity)
    {
        if (entity != _playerManager.LocalEntity || !Timing.IsFirstTimePredicted)
        {
            return;
        }

        var inCombatMode = IsInCombatMode();
        LocalPlayerCombatModeUpdated?.Invoke(inCombatMode);
    }

    private void OnShowCombatIndicatorsChanged(bool isShow)
    {
        if (isShow)
        {
            _overlayManager.AddOverlay(new CombatModeIndicatorsOverlay(
                _inputManager,
                EntityManager,
                _eye,
                this,
                EntityManager.System<HandsSystem>()));
        }
        else
        {
            _overlayManager.RemoveOverlay<CombatModeIndicatorsOverlay>();
        }
    }
}
