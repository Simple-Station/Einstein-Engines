// HarpyVisualSystem.cs by Tyler Chase Johnson (also known as VMSolidus) is marked CC0 1.0. To view a copy of this mark, visit https://creativecommons.org/publicdomain/zero/1.0/
using Robust.Shared.Serialization;

namespace Content.Shared.DeltaV.Harpy.Components
{
    [Serializable, NetSerializable]
    public enum HarpyVisualLayers
    {
        Singing,
    }

    [Serializable, NetSerializable]
    public enum SingingVisualLayer
    {
        True,
        False,
    }
}
