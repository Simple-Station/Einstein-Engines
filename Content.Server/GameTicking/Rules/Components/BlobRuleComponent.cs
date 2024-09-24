<<<<<<< HEAD:Content.Server/GameTicking/Rules/Components/BlobRuleComponent.cs
﻿using Content.Server.Blob;
using Content.Server.Roles;
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/GameTicking/Rules/Components/BlobRuleComponent.cs
﻿using Content.Server.Backmen.Blob;
using Content.Shared.Mind;
=======
﻿using Content.Server.Backmen.Blob;
using Content.Shared.Backmen.Blob.Components;
using Content.Shared.Mind;
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/GameTicking/Rules/Components/BlobRuleComponent.cs
using Robust.Shared.Audio;

namespace Content.Server.GameTicking.Rules.Components;

[RegisterComponent, Access(typeof(BlobRuleSystem), typeof(BlobCoreSystem))]
public sealed class BlobRuleComponent : Component
{
<<<<<<< HEAD:Content.Server/GameTicking/Rules/Components/BlobRuleComponent.cs
    public List<BlobRole> Blobs = new();

    public BlobStage Stage = BlobStage.Default;

    [DataField("alertAodio")]
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/GameTicking/Rules/Components/BlobRuleComponent.cs
    public List<(EntityUid mindId, MindComponent mind)> Blobs = new(); //BlobRoleComponent

    public BlobStage Stage = BlobStage.Default;

    [DataField("alertAodio")]
=======
    [DataField]
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/GameTicking/Rules/Components/BlobRuleComponent.cs
    public SoundSpecifier? AlertAudio = new SoundPathSpecifier("/Audio/Announcements/attention.ogg");

    [ViewVariables]
    public List<(EntityUid mindId, MindComponent mind)> Blobs = new(); //BlobRoleComponent

    [ViewVariables]
    public BlobStage Stage = BlobStage.Default;

    [ViewVariables]
    public float Accumulator = 0f;

    [ViewVariables]
    public Dictionary<EntityUid, HashSet<Entity<BlobCoreComponent>>> StationCores = [];
}


public enum BlobStage : byte
{
    Default,
    Begin,
    Critical,
    TheEnd,
}
