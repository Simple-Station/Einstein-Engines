using Content.Shared.Random;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Audio;

/// <summary>
/// Attaches a rules prototype to sound files to play ambience.
/// </summary>
[Prototype("ambientMusic")]
public sealed partial class AmbientMusicPrototype : IPrototype
{
    [IdDataField] public string ID { get; } = string.Empty;

    [ViewVariables(VVAccess.ReadWrite), DataField("sound", required: true)]
    public SoundSpecifier Sound = default!;

    /// <summary>
    /// Decides if this music will play on top of other music or not.
    /// NOTE!!! THIS ISN'T DONE YET!!!
    /// BIOMES: 1;
    /// SHIP AMBIENT: 2;
    /// COMBAT MODE: 3;
    /// FUCKED BIOMES/HADAL: 4;
    /// ADMIN: 5+;
    /// </summary>
    [DataField(required: false)]
    public int Priority = 1;

}
