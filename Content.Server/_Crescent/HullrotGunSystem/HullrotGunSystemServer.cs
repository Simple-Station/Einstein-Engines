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

    public static void SendProjectileEvent(Vector2 origin, Vector2 velocity, float timestamp)
    {
        if (_pipe is { IsConnected: true })
        {
            var msg = $"{origin.X},{origin.Y}|{velocity.X},{velocity.Y}|{timestamp}\n";
            var data = Encoding.UTF8.GetBytes(msg);
            _pipe.Write(data, 0, data.Length);
            _pipe.Flush();
        }
    }
}
