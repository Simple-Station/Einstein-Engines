using Robust.Shared.GameStates;
using Content.Shared.Damage;

namespace Content.Shared._Goobstation.Clothing.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class DamageOverTimeComponent : Component
{
    [DataField(required: true)]
    public DamageSpecifier Damage { get; set; } = new();

    [DataField]
    public TimeSpan Interval = TimeSpan.FromSeconds(1);

    [DataField]
    public bool IgnoreResistances { get; set; }

    [DataField]
    public TimeSpan NextTickTime = TimeSpan.Zero;
}
