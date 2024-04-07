namespace Content.Shared.Language.Components.Translators;

/// <summary>
///   A translator that must be held in a hand or a pocket of an entity in order ot have effect.
/// </summary>
[RegisterComponent]
public sealed partial class HandheldTranslatorComponent : Translators.BaseTranslatorComponent
{
    /// <summary>
    ///   Whether or not interacting with this translator
    ///   toggles it on or off.
    /// </summary>
    [DataField("toggleOnInteract")]
    public bool ToggleOnInteract = true;
}
