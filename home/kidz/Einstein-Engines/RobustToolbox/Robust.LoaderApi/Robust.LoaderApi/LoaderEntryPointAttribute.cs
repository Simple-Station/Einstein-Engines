using System;

namespace Robust.LoaderApi
{
    /// <summary>
    ///     Specifies which type in the assembly is responsible for being an engine loader.
    /// </summary>
    /// <remarks>
    ///     The type must implement <see cref="ILoaderEntryPoint"/>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Assembly)]
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class LoaderEntryPointAttribute : Attribute
    {
        public LoaderEntryPointAttribute(Type loaderEntryPointType)
        {
            LoaderEntryPointType = loaderEntryPointType;
        }

        public Type LoaderEntryPointType { get; }
    }
}
