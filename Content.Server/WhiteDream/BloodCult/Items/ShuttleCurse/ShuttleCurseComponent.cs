using Robust.Shared.Audio;

namespace Content.Server.WhiteDream.BloodCult.Items.ShuttleCurse;

[RegisterComponent]
public sealed partial class ShuttleCurseComponent : Component
{
    [DataField]
    public TimeSpan DelayTime = TimeSpan.FromMinutes(3);

    [DataField]
    public SoundSpecifier ScatterSound = new SoundCollectionSpecifier("GlassBreak");

    [DataField]
    public List<string> CurseMessages = new()
    {
        "A fuel technician just slit his own throat and begged for death.",
        "A scan of the shuttle's fuel tank has revealed tainting by a mixture of humanoid innards and teeth.",
        "A security incident involving a frenzied shuttle worker attacking coworkers with a laser cutter has just been reported as resolved by on-site security.",
        "A shuttle engineer began screaming 'DEATH IS NOT THE END' and ripped out wires until an arc flash seared off her flesh.",
        "A shuttle engineer was spotted inside the cockpit, feverishly arranging her innards on the floor in the shape of a rune before expiring.",
        "A shuttle inspector started laughing madly over the radio and then threw herself into an engine turbine.",
        "The corpse of an unidentified shuttle worker was found mutilated beyond recognition in the shuttle's main hold, with at least five unique sources of blood on the scene.",
        "The shuttle dispatcher was found dead with bloody symbols carved into their flesh.",
        "The shuttle's custodian was found washing the windows with their own blood.",
        "The shuttle's navigation programming was replaced by a file containing just two words: IT COMES.",
        "The shuttle's transponder is emitting the encoded message 'FEAR THE OLD BLOOD' in lieu of its assigned identification signal."
    };
}
