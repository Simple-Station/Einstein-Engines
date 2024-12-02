#region

using System.Threading;
using System.Threading.Tasks;
using Content.Client.IoC;
using Content.Client.Resources;
using JetBrains.Annotations;
using Robust.Client.Graphics;
using Robust.Shared.Utility;

#endregion


namespace Content.Client.Parallax.Data;


[UsedImplicitly, DataDefinition,]
public sealed partial class ImageParallaxTextureSource : IParallaxTextureSource
{
    /// <summary>
    ///     Texture path.
    /// </summary>
    [DataField("path", required: true)]
    public ResPath Path { get; private set; }

    Task<Texture> IParallaxTextureSource.GenerateTexture(CancellationToken cancel) =>
        Task.FromResult(StaticIoC.ResC.GetTexture(Path));
}
