using Content.Shared.DoAfter;
using Robust.Shared.Serialization;


namespace Content.Shared._EE.Shadowling;


/// <summary>
/// This handles Sling's DoAfter Events
/// </summary>
[Serializable, NetSerializable]
public sealed partial class EnthrallDoAfterEvent : SimpleDoAfterEvent { }
