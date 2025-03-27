namespace Content.Server.Weapons.Melee.EnergySword;

[RegisterComponent]
internal sealed partial class EnergySwordComponent : Component
{

    [ViewVariables(VVAccess.ReadWrite), DataField("activatedColor"), AutoNetworkedField]
    public Color ActivatedColor = Color.DodgerBlue;
    public int ColorChoice = 0;
    [ViewVariables(VVAccess.ReadWrite), DataField("activatedColorName"), AutoNetworkedField]
    public string ActivatedColorName = "Blue";

    /// <summary>
    ///     A color option list for the random color picker.
    /// </summary>
    [DataField("colorOptions")]
    public List<Color> ColorOptions = new()
    {
        Color.Tomato,
        Color.DodgerBlue,
        Color.Aqua,
        Color.MediumSpringGreen,
        Color.MediumOrchid
    };

    [DataField("colorNames")]
    public List<string> ColorNames = new()
    {
        "red-orange",
        "blue",
        "aqua",
        "medium green",
        "light purple",
    };

    public bool Hacked = false;
    /// <summary>
    ///     RGB cycle rate for hacked e-swords.
    /// </summary>
    [DataField("cycleRate")]
    public float CycleRate = 1f;
}
