using Content.Shared.Preferences;
using Content.Shared.Roles;
using JetBrains.Annotations;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Customization.Systems;

// ReSharper disable InvalidXmlDocComment
[ImplicitDataDefinitionForInheritors, MeansImplicitUse]
[Serializable, NetSerializable]
public abstract partial class CharacterRequirement
{
    /// <summary>
    ///     If true valid requirements will be treated as invalid and vice versa
    ///     This inversion is done by other systems like <see cref="CharacterRequirementsSystem"/>, not this one
    /// </summary>
    [DataField]
    public bool Inverted;

    /// <summary>
    ///     Checks if this character requirement is valid for the given parameters
    ///     <br />
    ///     You should probably not be calling this directly, use <see cref="CharacterRequirementsSystem"/>
    /// </summary>
    /// <param name="reason">Description for the requirement, shown when not null</param>
    public abstract bool IsValid(
        JobPrototype job,
        HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes,
        bool whitelisted,
        IPrototype prototype,
        IEntityManager entityManager,
        IPrototypeManager prototypeManager,
        IConfigurationManager configManager,
        out string? reason,
        int depth = 0
    );
}
