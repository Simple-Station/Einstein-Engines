using Content.Shared.DeviceLinking;
using Robust.Shared.Prototypes;

namespace Content.Server.LocalizedRadio.Prototypes
{
    // Forces all radios operating on a channel to be localized
    [Prototype("forceLocalize")]
    public sealed partial class ForceLocalizePrototype : IPrototype
    {
        /// <summary>
        ///  ID of the channel being forcefully localized
        /// </summary>
        [IdDataField, ViewVariables]
        public string ID { get; } = default!;
    }
}
