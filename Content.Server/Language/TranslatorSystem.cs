using System.Linq;
using Content.Server.Popups;
using Content.Server.PowerCell;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Language;
using Content.Shared.Language.Events;
using Content.Shared.Language.Systems;
using Content.Shared.PowerCell;
using Content.Shared.Language.Components.Translators;

namespace Content.Server.Language;

// This does not support holding multiple translators at once.
// That shouldn't be an issue for now, but it needs to be fixed later.
public sealed class TranslatorSystem : SharedTranslatorSystem
{
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly LanguageSystem _language = default!;
    [Dependency] private readonly PowerCellSystem _powerCell = default!;

    public override void Initialize()
    {
        base.Initialize();

        // I wanna die. But my death won't help us discover polymorphism.
        SubscribeLocalEvent<IntrinsicTranslatorComponent, DetermineEntityLanguagesEvent>(OnDetermineLanguages);
        SubscribeLocalEvent<HoldsTranslatorComponent, DetermineEntityLanguagesEvent>(OnDetermineLanguages);
        SubscribeLocalEvent<ImplantedTranslatorComponent, DetermineEntityLanguagesEvent>(OnDetermineLanguages);

        SubscribeLocalEvent<HandheldTranslatorComponent, ActivateInWorldEvent>(OnTranslatorToggle);
        SubscribeLocalEvent<HandheldTranslatorComponent, PowerCellSlotEmptyEvent>(OnPowerCellSlotEmpty);

        // TODO: why does this use InteractHandEvent??
        SubscribeLocalEvent<HandheldTranslatorComponent, InteractHandEvent>(OnTranslatorInteract);
        SubscribeLocalEvent<HandheldTranslatorComponent, DroppedEvent>(OnTranslatorDropped);
    }

    private void OnDetermineLanguages(EntityUid uid, IntrinsicTranslatorComponent component,
        DetermineEntityLanguagesEvent ev)
    {
        if (!component.Enabled)
            return;

        if (!_powerCell.HasActivatableCharge(uid))
            return;

        var addUnderstood = true;
        var addSpoken = true;
        if (component.RequiredLanguages.Count > 0)
        {
            if (component.RequiresAllLanguages)
            {
                // Add langs when the wielder has all of the required languages
                foreach (var language in component.RequiredLanguages)
                {
                    if (!ev.SpokenLanguages.Contains(language, StringComparer.Ordinal))
                        addSpoken = false;

                    if (!ev.UnderstoodLanguages.Contains(language, StringComparer.Ordinal))
                        addUnderstood = false;
                }
            }
            else
            {
                // Add langs when the wielder has at least one of the required languages
                addUnderstood = false;
                addSpoken = false;
                foreach (var language in component.RequiredLanguages)
                {
                    if (ev.SpokenLanguages.Contains(language, StringComparer.Ordinal))
                        addSpoken = true;

                    if (ev.UnderstoodLanguages.Contains(language, StringComparer.Ordinal))
                        addUnderstood = true;
                }
            }
        }

        if (addSpoken)
        {
            foreach (var language in component.SpokenLanguages)
                AddIfNotExists(ev.SpokenLanguages, language);

            if (component.DefaultLanguageOverride != null && ev.CurrentLanguage.Length == 0)
                ev.CurrentLanguage = component.DefaultLanguageOverride;
        }

        if (addUnderstood)
            foreach (var language in component.UnderstoodLanguages)
                AddIfNotExists(ev.UnderstoodLanguages, language);
    }

    private void OnTranslatorInteract( EntityUid translator, HandheldTranslatorComponent component, InteractHandEvent args)
    {
        var holder = args.User;
        if (!EntityManager.HasComponent<LanguageSpeakerComponent>(holder))
            return;

        var intrinsic = EnsureComp<HoldsTranslatorComponent>(holder);
        UpdateBoundIntrinsicComp(component, intrinsic, component.Enabled);

        RaiseLocalEvent(holder, new LanguagesUpdateEvent(), true);
    }

    private void OnTranslatorDropped(EntityUid translator, HandheldTranslatorComponent component, DroppedEvent args)
    {
        var holder = args.User;
        if (!EntityManager.TryGetComponent<HoldsTranslatorComponent>(holder, out var intrinsic))
            return;

        if (intrinsic.Issuer == component)
        {
            intrinsic.Enabled = false;
            RemCompDeferred(holder, intrinsic);
        }

        _language.EnsureValidLanguage(holder);

        RaiseLocalEvent(holder, new LanguagesUpdateEvent(), true);
    }

    private void OnTranslatorToggle(EntityUid translator, HandheldTranslatorComponent component, ActivateInWorldEvent args)
    {
        if (!component.ToggleOnInteract)
            return;

        var hasPower = _powerCell.HasDrawCharge(translator);

        if (Transform(args.Target).ParentUid is { Valid: true } holder
            && EntityManager.HasComponent<LanguageSpeakerComponent>(holder))
        {
            // This translator is held by a language speaker and thus has an intrinsic counterpart bound to it.
            // Make sure it's up-to-date.
            var intrinsic = EnsureComp<HoldsTranslatorComponent>(holder);
            var isEnabled = !component.Enabled;
            if (intrinsic.Issuer != component)
            {
                // The intrinsic comp wasn't owned by this handheld component, so this comp wasn't the active translator.
                // Thus it needs to be turned on regardless of its previous state.
                intrinsic.Issuer = component;
                isEnabled = true;
            }

            isEnabled &= hasPower;
            UpdateBoundIntrinsicComp(component, intrinsic, isEnabled);
            component.Enabled = isEnabled;
            _powerCell.SetPowerCellDrawEnabled(translator, isEnabled);

            _language.EnsureValidLanguage(holder);
            RaiseLocalEvent(holder, new LanguagesUpdateEvent(), true);
        }
        else
        {
            // This is a standalone translator (e.g. lying on the ground), toggle its state.
            component.Enabled = !component.Enabled && hasPower;
            _powerCell.SetPowerCellDrawEnabled(translator, !component.Enabled && hasPower);
        }

        OnAppearanceChange(translator, component);

        // HasPower shows a popup when there's no power, so we do not proceed in that case
        if (hasPower)
        {
            var message = Loc.GetString(
                component.Enabled
                    ? "translator-component-turnon"
                    : "translator-component-shutoff",
                ("translator", component.Owner));
            _popup.PopupEntity(message, component.Owner, args.User);
        }
    }

    private void OnPowerCellSlotEmpty(EntityUid translator, HandheldTranslatorComponent component, PowerCellSlotEmptyEvent args)
    {
        component.Enabled = false;
        _powerCell.SetPowerCellDrawEnabled(translator, false);
        OnAppearanceChange(translator, component);

        if (Transform(translator).ParentUid is { Valid: true } holder
            && EntityManager.HasComponent<LanguageSpeakerComponent>(holder))
        {
            if (!EntityManager.TryGetComponent<HoldsTranslatorComponent>(holder, out var intrinsic))
                return;

            if (intrinsic.Issuer == component)
            {
                intrinsic.Enabled = false;
                EntityManager.RemoveComponent(holder, intrinsic);
            }

            _language.EnsureValidLanguage(holder);
            RaiseLocalEvent(holder, new LanguagesUpdateEvent(), true);
        }
    }

    /// <summary>
    ///   Copies the state from the handheld to the intrinsic component
    /// </summary>
    private void UpdateBoundIntrinsicComp(HandheldTranslatorComponent comp, HoldsTranslatorComponent intrinsic, bool isEnabled)
    {
        if (isEnabled)
        {
            intrinsic.SpokenLanguages = new List<string>(comp.SpokenLanguages);
            intrinsic.UnderstoodLanguages = new List<string>(comp.UnderstoodLanguages);
            intrinsic.DefaultLanguageOverride = comp.DefaultLanguageOverride;
        }
        else
        {
            intrinsic.SpokenLanguages.Clear();
            intrinsic.UnderstoodLanguages.Clear();
            intrinsic.DefaultLanguageOverride = null;
        }

        intrinsic.Enabled = isEnabled;
        intrinsic.Issuer = comp;
    }

    private static void AddIfNotExists(List<string> list, string item)
    {
        if (list.Contains(item))
            return;
        list.Add(item);
    }
}
