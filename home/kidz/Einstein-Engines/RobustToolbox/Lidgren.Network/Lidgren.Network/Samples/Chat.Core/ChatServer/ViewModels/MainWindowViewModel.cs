using System;
using Avalonia.Threading;
using DynamicData;
using DynamicData.Binding;
using DynamicData.Kernel;
using Lidgren.Network;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ChatServer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public IObservableCollection<string> Messages { get; } = new ObservableCollectionExtended<string>();

        public IObservableCollection<ConnectedClient> Clients { get; } =
            new ObservableCollectionExtended<ConnectedClient>();

        public NetPeer NetPeer => _server;

        private NetServer _server;
        private DispatcherTimer _timer;

        private readonly SourceList<string> _messages = new SourceList<string>();
        private readonly SourceCache<ConnectedClient, NetConnection> _clients
            = new SourceCache<ConnectedClient, NetConnection>(p => p.Connection);

        [Reactive] public bool Running { get; set; }
        public string StartText => Running ? "Shut down" : "Start";

        public MainWindowViewModel()
        {
            _messages.Connect()
                .Bind(Messages)
                .Subscribe();

            _clients.Connect()
                .Bind(Clients)
                .Subscribe();

            this.WhenAnyValue(p => p.Running)
                .Subscribe(_ =>
                {
                    this.RaisePropertyChanged(nameof(StartText));
                });
        }

        public void Initialized()
        {
            // set up network
            var config = new NetPeerConfiguration("chat")
            {
                MaximumConnections = 100,
                Port = 14242
            };

            _server = new NetServer(config);
            _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(1), DispatcherPriority.ApplicationIdle, UpdateNet);
            _timer.Start();
        }

        private void UpdateNet(object sender, EventArgs e)
        {
            NetIncomingMessage im;
            while ((im = _server.ReadMessage()) != null)
            {
                // handle incoming message
                switch (im.MessageType)
                {
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.ErrorMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.VerboseDebugMessage:
                        var text = im.ReadString();
                        Output(text);
                        break;

                    case NetIncomingMessageType.StatusChanged:
                        var status = (NetConnectionStatus) im.ReadByte();

                        var reason = im.ReadString();
                        Output(NetUtility.ToHexString(im.SenderConnection.RemoteUniqueIdentifier) + " " + status +
                               ": " + reason);

                        if (status == NetConnectionStatus.Connected)
                            Output("Remote hail: " + im.SenderConnection.RemoteHailMessage.ReadString());

                        if (status == NetConnectionStatus.Disconnected)
                        {
                            _clients.Remove(im.SenderConnection);
                            break;
                        }
                        var existing = _clients.Lookup(im.SenderConnection);
                        if (!existing.HasValue)
                        {
                            var client = new ConnectedClient(im.SenderConnection);
                            _clients.AddOrUpdate(client);
                            existing = Optional<ConnectedClient>.Create(client);
                        }

                        existing.Value.Status = status;
                        break;

                    case NetIncomingMessageType.Data:
                        // incoming chat message from a client
                        var chat = im.ReadString();

                        Output($"Broadcasting '{chat}'");

                        // broadcast this to all connections, except sender
                        var all = _server.Connections; // get copy
                        all.Remove(im.SenderConnection);

                        if (all.Count > 0)
                        {
                            var om = _server.CreateMessage();
                            om.Write(NetUtility.ToHexString(im.SenderConnection.RemoteUniqueIdentifier) + " said: " +
                                     chat);
                            _server.SendMessage(om, all, NetDeliveryMethod.ReliableOrdered, 0);
                        }

                        break;
                    default:
                        Output(
                            $"Unhandled type: {im.MessageType} {im.LengthBytes} bytes " +
                            $"{im.DeliveryMethod}|{im.SequenceChannel}");
                        break;
                }

                _server.Recycle(im);
            }
        }

        public void StartServer()
        {
            if (Running)
            {
                Running = false;
                Shutdown();
            }
            else
            {
                Running = true;
                _server.Start();
            }
        }

        public void Shutdown()
        {
            _server.Shutdown("Requested by user");
        }

        public void Exiting()
        {
            Shutdown();
            _timer.Stop();
        }

        private void Output(string text)
        {
            _messages.Add(text);
        }
    }
}