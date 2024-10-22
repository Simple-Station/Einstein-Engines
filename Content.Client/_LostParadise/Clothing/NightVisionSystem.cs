using Content.Shared._LostParadise.Clothing;
using Content.Shared.GameTicking;
using Robust.Client.Player;
using Robust.Client.Graphics;
using Content.Client.Inventory;
using Content.Shared.Inventory.Events;

namespace Content.Client._LostParadise.Clothing;

/// <summary>
/// Made by BL02DL from _LostParadise
/// </summary>

public sealed class NightVisionSystem : SharedNightVisionSystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly ILightManager _lightManager = default!;

    private NightVisionOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRestart);
        SubscribeLocalEvent<NightVisionComponent, GotUnequippedEvent>(OnGotUnequipped);

        _overlay = new();
    }

    protected override void UpdateNightVisionEffects(EntityUid parent, EntityUid uid, bool state, NightVisionComponent? component)
    {
        if (!Resolve(uid, ref component))
        {
            return;
        }

        state = state && component.On;

        if (state)
        {
            _lightManager.DrawLighting = false;

            /// Adding an overlay when the effect is activated
            /// Добавление наложения при активированном эффекте
            _overlayMan.AddOverlay(_overlay);
        }
        else
        {
            _lightManager.DrawLighting = true;

            /// Removing an overlay when the effect is deactivated
            /// Удаление наложения при деактивированном эффекте
            _overlayMan.RemoveOverlay(_overlay);
        }
    }
    private void OnGotUnequipped(EntityUid uid, NightVisionComponent component, GotUnequippedEvent args)
    {
        if (args.Slot == "eyes")
        {
            UpdateNightVisionEffects(args.Equipee, uid, false, component);
            _overlayMan.RemoveOverlay(_overlay);
            _lightManager.DrawLighting = true;
        }
    }
    private void OnRestart(RoundRestartCleanupEvent ev)
    {
        /// Удаляем оверлей и врубаем свет на всякий по окончанию раунда
        /// We remove the overlay and turn on the light just in case at the end of the round.
        _overlayMan.RemoveOverlay(_overlay);
        _lightManager.DrawLighting = true;
    }
}

public sealed class NightVisionEntitySystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    public override void Initialize()
    {
        base.Initialize();
    }

    public NightVisionComponent? GetNightComp()
    {
        var playerUid = EntityUid.Parse(_playerManager.LocalPlayer?.ControlledEntity.ToString());
        var slot = _entityManager.GetComponent<InventorySlotsComponent>(playerUid);
        _entityManager.TryGetComponent<NightVisionComponent>(slot.SlotData["eyes"].HeldEntity, out var nightvision);
        return nightvision;
    }
}