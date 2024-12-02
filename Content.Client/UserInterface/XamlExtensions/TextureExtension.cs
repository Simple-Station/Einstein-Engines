#region

using Content.Client.Resources;
using JetBrains.Annotations;
using Robust.Client.ResourceManagement;

#endregion


namespace Content.Client.UserInterface.XamlExtensions;


[PublicAPI]
public sealed class TexExtension
{
    private IResourceCache _resourceCache;
    public string Path { get; }

    public TexExtension(string path)
    {
        _resourceCache = IoCManager.Resolve<IResourceCache>();
        Path = path;
    }

    public object ProvideValue() => _resourceCache.GetTexture(Path);
}
