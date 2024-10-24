using System.Reactive.Linq;
using Lidgren.Network;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ChatServer.ViewModels
{
    public sealed class ConnectedClient : ReactiveObject
    {
        public ConnectedClient(NetConnection connection)
        {
            Connection = connection;

            this.WhenAnyValue(p => p.Status)
                .Select(p =>
                {
                    var str = $"{NetUtility.ToHexString(Connection.RemoteUniqueIdentifier)} " +
                              $"from {Connection.RemoteEndPoint} [{Connection.Status}]";
                    return str;
                })
                .ToPropertyEx(this, client => client.EntryText);
        }

        public NetConnection Connection { get; }
        [Reactive] public NetConnectionStatus Status { get; set; }
        public string EntryText { [ObservableAsProperty] get; }
    }
}