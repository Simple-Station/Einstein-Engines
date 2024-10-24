using System;
using System.Text;
using Avalonia.Controls;
using Avalonia.Threading;
using Lidgren.Network;
using ReactiveUI;

namespace SamplesCommon.Core
{
    public partial class NetPeerSettingsWindow : Window
    {
        private readonly NetPeer _peer;
        private readonly DispatcherTimer _timer;

        public NetPeerSettingsWindow()
        {
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            _timer.Stop();
        }

        private void UpdateStatistics(object sender, EventArgs e)
        {
            var bdr = new StringBuilder();
            bdr.AppendLine(_peer.Statistics.ToString());

            if (_peer.ConnectionsCount > 0)
            {
                var conn = _peer.Connections[0];
                bdr.AppendLine("Connection 0:");
                bdr.Append(conn.Statistics.ToString());
            }

            Statistics.Text = bdr.ToString();
        }

        public NetPeerSettingsWindow(NetPeer peer)
        {
            InitializeComponent();

            _peer = peer;
            _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(250), DispatcherPriority.ApplicationIdle,
                (sender, e) => UpdateStatistics());
            _timer.Start();

            SaveButton.Command = ReactiveCommand.Create(SaveButtonPressed);
            CloseButton.Command = ReactiveCommand.Create(CloseButtonPressed);

            UpdateLabelsAndBoxes();
        }

        private void UpdateStatistics()
        {
            var bdr = new StringBuilder();
            bdr.AppendLine(_peer.Statistics.ToString());

            if (_peer.ConnectionsCount > 0)
            {
                var conn = _peer.Connections[0];
                bdr.AppendLine("Connection 0:");
                bdr.Append(conn.Statistics);
            }

            Statistics.Text = bdr.ToString();
        }

        private void CloseButtonPressed()
        {
            Close();
        }

        private void SaveButtonPressed()
        {
            Save();
            UpdateLabelsAndBoxes();
            UpdateStatistics();
        }

        private void Save()
        {
	        _peer.Configuration.SetMessageTypeEnabled(NetIncomingMessageType.ErrorMessage,
		        ErrorCheckBox.IsChecked.Value);
	        _peer.Configuration.SetMessageTypeEnabled(NetIncomingMessageType.WarningMessage,
		        WarningCheckBox.IsChecked.Value);
	        _peer.Configuration.SetMessageTypeEnabled(NetIncomingMessageType.DebugMessage,
                DebugCheckBox.IsChecked.Value);
            _peer.Configuration.SetMessageTypeEnabled(NetIncomingMessageType.VerboseDebugMessage,
                VerboseCheckBox.IsChecked.Value);
#if DEBUG
            if (float.TryParse(LossBox.Text, out var f))
                _peer.Configuration.SimulatedLoss = f / 100f;
            if (float.TryParse(DuplicatesBox.Text, out f))
                _peer.Configuration.SimulatedDuplicatesChance = f / 100f;
            if (float.TryParse(MinLag.Text, out f))
                _peer.Configuration.SimulatedMinimumLatency = f / 1000f;
            if (float.TryParse(PingBox.Text, out f))
                _peer.Configuration.PingInterval = f / 1000f;
            if (float.TryParse(MaxLag.Text, out var max))
            {
                max /= 1000f;
                var r = max - _peer.Configuration.SimulatedMinimumLatency;
                if (r > 0)
                {
                    _peer.Configuration.SimulatedRandomLatency = r;
                    var nm = _peer.Configuration.SimulatedMinimumLatency +
                             _peer.Configuration.SimulatedRandomLatency;
                    MaxLag.Text = ((int) (max * 1000)).ToString();
                }
            }
#endif
        }

        private void UpdateLabelsAndBoxes()
        {
            var pc = _peer.Configuration;

#if DEBUG
            var loss = (pc.SimulatedLoss * 100.0f).ToString();
            LossPercent.Text = $"{loss} %";
            LossBox.Text = loss;

            var dupes = (pc.SimulatedDuplicatesChance * 100.0f).ToString();
            DuplicatesPercent.Text = $"{dupes} %";
            DuplicatesBox.Text = dupes;

            var minLat = (pc.SimulatedMinimumLatency * 1000.0f).ToString();
            var maxLat = ((pc.SimulatedMinimumLatency + pc.SimulatedRandomLatency) * 1000.0f).ToString();
#else
			var minLat = "";
			var maxLat = "";
#endif
            DelayDisplay.Text = $"{minLat} to {maxLat} ms";
            MinLag.Text = minLat;
            MaxLag.Text = maxLat;

            ErrorCheckBox.IsChecked = _peer.Configuration.IsMessageTypeEnabled(NetIncomingMessageType.ErrorMessage);
            WarningCheckBox.IsChecked = _peer.Configuration.IsMessageTypeEnabled(NetIncomingMessageType.WarningMessage);
            DebugCheckBox.IsChecked = _peer.Configuration.IsMessageTypeEnabled(NetIncomingMessageType.DebugMessage);
            VerboseCheckBox.IsChecked =
                _peer.Configuration.IsMessageTypeEnabled(NetIncomingMessageType.VerboseDebugMessage);
            PingBox.Text = (_peer.Configuration.PingInterval * 1000).ToString();
        }
    }
}