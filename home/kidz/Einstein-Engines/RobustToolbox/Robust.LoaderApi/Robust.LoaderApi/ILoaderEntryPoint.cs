namespace Robust.LoaderApi
{
    /// <summary>
    ///     Implemented by robust in <c>Robust.Client</c> to act as an entry point into the engine.
    /// </summary>
    /// <seealso cref="LoaderEntryPointAttribute"/>
    public interface ILoaderEntryPoint
    {
        /// <summary>
        ///     The main entry point of the engine.
        /// </summary>
        void Main(IMainArgs args);
    }
}
