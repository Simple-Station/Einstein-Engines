#region

using System.Linq;
using Content.Shared.SprayPainter;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Serialization.TypeSerializers.Implementations;
using Robust.Shared.Utility;

#endregion


namespace Content.Client.SprayPainter;


public sealed class SprayPainterSystem : SharedSprayPainterSystem
{
    [Dependency] private readonly IResourceCache _resourceCache = default!;

    public List<SprayPainterEntry> Entries { get; } = new();

    protected override void CacheStyles()
    {
        base.CacheStyles();

        Entries.Clear();
        foreach (var style in Styles)
        {
            var name = style.Name;
            var iconPath = Groups
                .FindAll(x => x.StylePaths.ContainsKey(name))
                ?
                .MaxBy(x => x.IconPriority)
                ?.StylePaths[name];
            if (iconPath == null)
            {
                Entries.Add(new(name, null));
                continue;
            }

            var doorRsi =
                _resourceCache.GetResource<RSIResource>(SpriteSpecifierSerializer.TextureRoot / new ResPath(iconPath));
            if (!doorRsi.RSI.TryGetState("closed", out var icon))
            {
                Entries.Add(new(name, null));
                continue;
            }

            Entries.Add(new(name, icon.Frame0));
        }
    }
}

public sealed class SprayPainterEntry
{
    public string Name;
    public Texture? Icon;

    public SprayPainterEntry(string name, Texture? icon)
    {
        Name = name;
        Icon = icon;
    }
}
