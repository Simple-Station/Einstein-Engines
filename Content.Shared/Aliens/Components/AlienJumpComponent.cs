using Content.Shared.Actions;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Aliens.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class AlienJumpComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public EntProtoId Action = "ActionJumpAlien";

    [DataField("actionEntity")]
    public EntityUid? ActionEntity;

    [DataField]
    public float JumpTime = 1f;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("jumpSprite")]
    public ResPath JumpSprite { get; set; }

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("sprite")]
    public ResPath Sprite { get; set; }

    [ViewVariables]
    public SpriteSpecifier? OldSprite;

    [ViewVariables]
    public bool Hit = false;
}

public sealed partial class AlienJumpActionEvent : WorldTargetActionEvent { }

[NetSerializable]
[Serializable]
public enum JumpVisuals : byte
{
    Jumping
}
