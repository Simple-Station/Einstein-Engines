namespace Content.Shared.Language.Components.Translators;

/// <summary>
///     Applied internally to the holder of an entity with [HandheldTranslatorComponent].
/// </summary>
[RegisterComponent]
public sealed partial class HoldsTranslatorComponent : Component
{
    [NonSerialized]
    public HashSet<Entity<HandheldTranslatorComponent>> Translators = new();
}
