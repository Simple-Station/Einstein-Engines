namespace Content.Shared.Language.Components.Translators;

/// <summary>
///   Applied internally to the holder of [HandheldTranslatorComponent].
///   Do not use directly. Use [HandheldTranslatorComponent] instead.
/// </summary>
[RegisterComponent]
public sealed partial class HoldsTranslatorComponent : IntrinsicTranslatorComponent
{
    public Component? Issuer = null;
}
