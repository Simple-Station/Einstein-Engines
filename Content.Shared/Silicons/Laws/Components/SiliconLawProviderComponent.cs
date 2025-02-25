using Robust.Shared.Prototypes;
using Robust.Shared.Audio;

namespace Content.Shared.Silicons.Laws.Components;

/// <summary>
/// This is used for an entity which grants laws to a <see cref="SiliconLawBoundComponent"/>
/// </summary>
[RegisterComponent, Access(typeof(SharedSiliconLawSystem))]
public sealed partial class SiliconLawProviderComponent : Component
{
    /// <summary>
    /// The id of the lawset that is being provided.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<SiliconLawsetPrototype> Laws = string.Empty;

    /// <summary>
    /// Lawset created from the prototype id.
    /// Cached when getting laws and modified during an ion storm event and when emagged.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public SiliconLawset? Lawset;

    /// <summary>
    /// The sound that plays for the Silicon player
    /// when the particular lawboard has been inserted.
    /// </summary>
    [DataField]
    public SoundSpecifier? LawUploadSound = new SoundPathSpecifier("/Audio/Misc/cryo_warning.ogg");

    /// <summary>
    ///     Whether this lawset cannot ever be removed, such as Antagonistic laws. This is largely needed for MALF AI,
    ///     since it is imperative that the MALF AI needs to be destroyed, and can't simply be rendered harmless by
    ///     the research director getting a board out of his closet.
    /// </summary>
    [DataField]
    public bool UnRemovable;

}
