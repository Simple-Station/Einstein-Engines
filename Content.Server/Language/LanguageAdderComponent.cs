using System.ComponentModel.DataAnnotations;
using Content.Shared.Language;
using Robust.Shared.Prototypes;

namespace Content.Server.Language;

/// <summary>
///    HULLROT: adds new languages without overriding existing ones, because LanguageKnowledgeComponent does that.
/// </summary>
[RegisterComponent]
public sealed partial class LanguageAdderComponent : Component
{
    /// <summary>
    /// HULLROT: this is used to add a new language without overriding the other languages this entity knows
    /// this is mainly used to give species their own specific languages, and then give jobs/factions their language afterward
    /// </summary>
    [DataField("addSpoken", required: false)]
    public List<ProtoId<LanguagePrototype>> AddedSpokenLanguages = new();

    /// <summary>
    /// HULLROT: this is used to add a new language without overriding the other languages this entity knows
    /// this is mainly used to give species their own specific languages, and then give jobs/factions their language afterward
    /// </summary>
    [DataField("addUnderstood", required: false)]
    public List<ProtoId<LanguagePrototype>> AddedUnderstoodLanguages = new();

}
