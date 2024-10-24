using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Robust.LoaderApi
{
    /// <summary>
    ///     API that the engine can use to invoke the launcher (if supported)
    /// </summary>
    public interface IRedialApi
    {
        /// <summary>
        ///     Try to cause the launcher to either reconnect to the same server or connect to a new server.
        ///     *The engine should shutdown on success, and this should be enforced in RobustToolbox.*
        ///     Will throw an exception if contacting the launcher failed (success indicates it is now the launcher's responsibility).
        ///     *It is expected that the engine use the --ss14-address parameter for redialling to the same server.*
        /// </summary>
        /// <param name="uri">The server Uri, such as "ss14://localhost:1212/".</param>
        /// <param name="text">Informational text on the cause of the reconnect. Empty gives a default reason.</param>
        void Redial(Uri uri, string text = "");
    }
}
