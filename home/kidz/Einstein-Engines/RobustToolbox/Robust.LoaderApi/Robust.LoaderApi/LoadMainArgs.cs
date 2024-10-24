using System.Collections.Generic;

namespace Robust.LoaderApi
{
    /// <summary>
    ///     Contains various thing that the loader needs to communicate to the engine.
    /// </summary>
    public interface IMainArgs
    {
        /// <summary>
        ///     Command line arguments to pass to the engine.
        /// </summary>
        string[] Args { get; }

        /// <summary>
        ///     File API usable to read files from the engine's installation directory.
        /// </summary>
        IFileApi FileApi { get; }

        /// <summary>
        ///     Redialling API usable to cause a connection to a new (or the same) server. Only provided if supported.
        /// </summary>
        IRedialApi? RedialApi => null;

        /// <summary>
        /// Extra file APIs to mount into the VFS.
        /// </summary>
        IEnumerable<ApiMount>? ApiMounts => null;
    }
}
