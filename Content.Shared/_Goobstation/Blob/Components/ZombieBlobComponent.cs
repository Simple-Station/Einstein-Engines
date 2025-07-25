﻿using Content.Shared.Antag;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.StatusIcon;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Blob.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ZombieBlobComponent : Component
{
    public List<string> OldFactions = new();

    [AutoNetworkedField]
    public EntityUid BlobPodUid = default!;

    public float? OldColdDamageThreshold = null;

    [ViewVariables]
    public Dictionary<string, int> DisabledFixtureMasks { get; } = new();

    [DataField("greetSoundNotification")]
    public SoundSpecifier GreetSoundNotification = new SoundPathSpecifier("/Audio/Ambience/Antag/zombie_start.ogg");

    [DataField, AutoNetworkedField]
    public bool CanShoot = false;
}
