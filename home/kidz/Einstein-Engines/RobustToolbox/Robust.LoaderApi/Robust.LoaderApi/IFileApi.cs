using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Robust.LoaderApi
{
    /// <summary>
    ///     API that the engine can use to access files in its installation directory,
    ///     since the engine may be loaded from a zip file.
    /// </summary>
    public interface IFileApi
    {
        /// <summary>
        ///     Try to read a file from the engine installation.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <param name="stream">The file stream, if the file was found.</param>
        /// <returns>True if the file exists and was opened, false otherwise.</returns>
        bool TryOpen(string path, [NotNullWhen(true)] out Stream? stream);

        /// <summary>
        ///     An enumerable of all files in the engine installation.
        /// </summary>
        /// <remarks>
        ///     The engine is expected to cache this.
        /// </remarks>
        IEnumerable<string> AllFiles { get; }
    }
}
