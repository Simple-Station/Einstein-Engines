#region

using JetBrains.Annotations;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Utility;

#endregion


namespace Content.Client.Resources;


[PublicAPI]
public static class ResourceCacheExtensions
{
    public static Texture GetTexture(this IResourceCache cache, ResPath path) =>
        cache.GetResource<TextureResource>(path);

    public static Texture GetTexture(this IResourceCache cache, string path) => GetTexture(cache, new ResPath(path));

    public static Font GetFont(this IResourceCache cache, ResPath path, int size) =>
        new VectorFont(cache.GetResource<FontResource>(path), size);

    public static Font GetFont(this IResourceCache cache, string path, int size) =>
        cache.GetFont(new ResPath(path), size);

    public static Font GetFont(this IResourceCache cache, ResPath[] path, int size)
    {
        var fs = new Font[path.Length];
        for (var i = 0; i < path.Length; i++)
            fs[i] = new VectorFont(cache.GetResource<FontResource>(path[i]), size);

        return new StackedFont(fs);
    }

    public static Font GetFont(this IResourceCache cache, string[] path, int size)
    {
        var rp = new ResPath[path.Length];
        for (var i = 0; i < path.Length; i++)
            rp[i] = new(path[i]);

        return cache.GetFont(rp, size);
    }
}
