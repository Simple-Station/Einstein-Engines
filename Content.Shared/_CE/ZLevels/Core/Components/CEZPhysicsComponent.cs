/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using System.Numerics;
using Content.Shared._CE.ZLevels.Core.EntitySystems;
using Robust.Shared.GameStates;

namespace Content.Shared._CE.ZLevels.Core.Components;

/// <summary>
/// Allows an entity to move up and down the z-levels by gravity or jumping
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true),
 Access(typeof(CESharedZLevelsSystem))]
public sealed partial class CEZPhysicsComponent : Component
{
    /// <summary>
    /// The current speed of movement between z-levels.
    /// If greater than 0, the entity moves upward. If less than 0, the entity moves downward.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Velocity;

    /// <summary>
    /// The current height of the entity within the current Z-level.
    /// Takes values from 0 to 1. If the value rises above 1, the entity moves up to the next level and the value is normalized.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float LocalPosition;

    /// Optimization Caches

    /// <summary>
    /// Cached value of the current z-level map height
    /// </summary>
    [DataField, AutoNetworkedField]
    public int CurrentZLevel;

    /// <summary>
    /// Cached value of the current distance to the ground in the current z-level. Updates only on MoveEvent and when tiles below change.
    /// </summary>
    [DataField]
    public float CurrentGroundHeight;

    /// <summary>
    /// Cached value of whether the entity is currently on sticky ground (ladders).
    /// </summary>
    [DataField]
    public bool CurrentStickyGround;

    // Physics

    [DataField, AutoNetworkedField]
    public float Bounciness = 0.3f;

    [DataField, AutoNetworkedField]
    public float GravityMultiplier = 1f;

    // Visuals

    /// <summary>
    /// Used only by the client.
    /// Blocks the rotation of an object if it has <see cref="LocalPosition"/> > 0,
    /// and saves the original NoRot value in SpriteComponent here so that it can be restored in the future.
    /// </summary>
    [DataField]
    public bool NoRotDefault;

    /// <summary>
    /// The original DrawDepth of the object is automatically saved here. Increases by 1 when the creature has <see cref="LocalPosition"/> > 0
    /// </summary>
    [DataField]
    public int DrawDepthDefault;

    /// <summary>
    /// When the mapinit entity is created, its initial Sprite Offset value is written here in order to apply an offset based on the Z position relative to this value.
    /// </summary>
    [DataField]
    public Vector2 SpriteOffsetDefault = Vector2.Zero;

    /// <summary>
    /// automatically rises if the current localPosition is lower than the height. Enabled by default, but for ghosts, for example, there is no point in climbing stairs
    /// </summary>
    [DataField]
    public bool AutoStep = true;
}
