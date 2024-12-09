using Content.Shared.Targeting;

namespace Content.Shared.Projectiles;

/// <summary>
/// Raised directed on an entity when it embeds in another entity.
/// </summary>
[ByRefEvent]
public readonly record struct EmbedEvent(EntityUid? Shooter, EntityUid Embedded, TargetBodyPart? BodyPart)
{
    public readonly EntityUid? Shooter = Shooter;

    /// <summary>
    /// Entity that is embedded in.
    /// </summary>
    public readonly EntityUid Embedded = Embedded;

    /// <summary>
    ///   Body part that has the embedded entity.
    /// </summary>
    public readonly TargetBodyPart? BodyPart = BodyPart;
}

/// <summary>
/// Raised on an entity when it stops embedding in another entity.
/// </summary>
[ByRefEvent]
public readonly record struct RemoveEmbedEvent(EntityUid? Remover)
{
    public readonly EntityUid? Remover = Remover;
}
