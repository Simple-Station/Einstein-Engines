using Robust.Shared.Audio;

namespace Content.Shared.Silicons.Bots;

/// <summary>
/// Used by the server for NPC Plantbot servicing.
/// Currently no clientside prediction done, only exists in shared for emag handling.
/// </summary>
[RegisterComponent]
[Access(typeof(PlantbotSystem))]
public sealed partial class PlantbotComponent : Component
{
    /// <summary>
    /// Sound played after watering a plantHolder.
    /// </summary>
    [DataField]
    public SoundSpecifier WaterSound = new SoundPathSpecifier("/Audio/Effects/Fluids/watersplash.ogg");

    /// <summary>
    /// Sound played after weeding a plantHolder.
    /// </summary>
    [DataField]
    public SoundSpecifier WeedSound = new SoundPathSpecifier("/Audio/Effects/plant_rustle.ogg");

    /// <summary>
    /// Sound played after draining a plantHolder.
    /// </summary>
    [DataField]
    public SoundSpecifier RemoveWaterSound = new SoundPathSpecifier("/Audio/Items/drink.ogg");

    [DataField]
    public SoundSpecifier EmagSparkSound = new SoundCollectionSpecifier("sparks")
    {
        Params = AudioParams.Default.WithVolume(8f)
    };

    public bool IsEmagged = false;
}
