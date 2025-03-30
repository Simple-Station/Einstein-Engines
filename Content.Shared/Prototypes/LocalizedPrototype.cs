using System.Linq;
using Robust.Shared.Prototypes;

namespace Content.Shared.Prototypes;

public abstract class LocalizedPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = default!;

    public const string LocFormat = "{0}-{1}-{2}";

    /// <summary>The localization string for the name of this prototype</summary>
    public string NameLoc => ToLocalizationString("name");
    /// <summary>The localized string for the name of prototype</summary>
    public string Name => Loc.GetString(NameLoc);

    /// <summary>
    ///     Returns an Loc string using the <see cref="field"/> as the 'property'.
    ///     Given `desc` it will return `this-prototype-PrototypeId-desc`.
    /// </summary>
    public string ToLocalizationString(string field)
    {
        // Get the ID of the proto Type
        var type =
            ((PrototypeAttribute?) Attribute.GetCustomAttribute(GetType(), typeof(PrototypeAttribute)))?.Type
            ?? GetType().Name.Remove(GetType().Name.Length - 9);
        // Lowercase the first letter
        type = OopsConcat(char.ToLowerInvariant(type[0]).ToString(), type[1..]);
        // Replace every uppercase letter with a dash and the lowercase letter
        type = type.Aggregate("", (current, c) => current + (char.IsUpper(c) ? OopsConcat("-", char.ToLowerInvariant(c).ToString()) : c.ToString()));

        return string.Format(LocFormat, type, ID, field);
    }

    private static string OopsConcat(string a, string b)
    {
        // This exists to prevent Roslyn being clever and compiling something that fails sandbox checks.
        return a + b;
    }
}
