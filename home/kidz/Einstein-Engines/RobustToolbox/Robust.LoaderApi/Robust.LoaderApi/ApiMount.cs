namespace Robust.LoaderApi
{
    public sealed record ApiMount(IFileApi FileApi, string MountPrefix);
}
