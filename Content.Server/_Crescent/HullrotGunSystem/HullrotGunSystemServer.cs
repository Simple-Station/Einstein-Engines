using System.IO.Pipes;

namespace Content.Server._Crescent.HullrotGunSystem;


/// <summary>
/// This handles...
/// </summary>
public sealed class HullrotGunSystemServer : EntitySystem
{
    private static NamedPipeServerStream? _InputPipe;
    /// <inheritdoc/>
    public override void Initialize()
    {
        _InputPipe = new NamedPipeServerStream("ProjectileHook", PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        _InputPipe.WaitForConnectionAsync(); // Don't block main thread

    }
}
