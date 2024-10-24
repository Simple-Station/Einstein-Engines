using System;
using DynamicData;
using DynamicData.Binding;
using Lidgren.Network;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ChatClient.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        [Reactive] public string ConnectAddress { get; set; } = "localhost";
        [Reactive] public string ConnectPort { get; set; } = "14242";
        [Reactive] public string TextBox { get; set; } = "";

        private readonly SourceList<string> _messageList = new SourceList<string>();
        public IObservableCollection<string> Messages { get; } = new ObservableCollectionExtended<string>();

        private NetClient _client;

        public NetPeer NetPeer => _client;

        [Reactive] public bool Connected { get; set; }
        public string ConnectText => Connected ? "Disconnect" : "Connect";

        public MainWindowViewModel()
        {
            _messageList
                .Connect()
                .Bind(Messages)
                .Subscribe();

            this.WhenAnyValue(p => p.Connected)
                .Subscribe(_ =>
                {
                    this.RaisePropertyChanged(nameof(ConnectText));
                });
        }

        public void ConnectPressed()
        {
            if (Connected)
            {
                Connected = false;
                _client.Shutdown("Bye");
            }
            else
            {
                int.TryParse(ConnectPort, out var port);
                Connect(ConnectAddress, port);
                Connected = true;
            }
        }

        public void SendPressed()
        {
            var om = _client.CreateMessage(TextBox);
            _client.SendMessage(om, NetDeliveryMethod.ReliableOrdered);
            Output($"Sending '{TextBox}'");
            _client.FlushSendQueue();
            TextBox = "";
        }

        public void Initialized()
        {
            var config = new NetPeerConfiguration("chat")
            {
                AutoFlushSendQueue = false
            };
            _client = new NetClient(config);
            _client.RegisterReceivedCallback(GotMessage);
        }

        private void GotMessage(object state)
        {
            NetIncomingMessage im;
            while ((im = _client.ReadMessage()) != null)
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

                        /*
                        if (status == NetConnectionStatus.Connected)
                            s_form.EnableInput();
                        else
                            s_form.DisableInput();

                        if (status == NetConnectionStatus.Disconnected)
                            s_form.button2.Text = "Connect";
                            */

                        var reason = im.ReadString();
                        Output($"{status}: {reason}");
                        break;
                    case NetIncomingMessageType.Data:
                        var chat = im.ReadString();
                        Output(chat);
                        break;
                    default:
                        Output($"Unhandled type: {im.MessageType} {im.LengthBytes} bytes");
                        break;
                }

                _client.Recycle(im);
            }
        }

        private void Output(string text)
        {
            _messageList.Add(text);
        }

        public void Exiting()
        {
            _client?.Shutdown("Bye");
        }

        private void Connect(string host, int port)
        {
            _client.Start();
            var hail = _client.CreateMessage("This is the hail message");
            _client.Connect(host, port, hail);
        }
    }
}