#region

using Robust.Client.ResourceManagement;

#endregion


namespace Content.Client.IoC;


public static class StaticIoC
{
    public static IResourceCache ResC => IoCManager.Resolve<IResourceCache>();
}
