﻿#region

using System.Text.RegularExpressions;
using static Content.Shared.Configurable.ConfigurationComponent;

#endregion


namespace Content.Client.Configurable.UI
{
    public sealed class ConfigurationBoundUserInterface : BoundUserInterface
    {
        [ViewVariables]
        private ConfigurationMenu? _menu;

        [ViewVariables]
        public Regex? Validation { get; internal set; }

        public ConfigurationBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey) { }

        protected override void Open()
        {
            base.Open();
            _menu = new ConfigurationMenu(this);

            _menu.OnClose += Close;
            _menu.OpenCentered();
        }

        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);

            if (state is not ConfigurationBoundUserInterfaceState configurationState)
            {
                return;
            }

            _menu?.Populate(configurationState);
        }

        protected override void ReceiveMessage(BoundUserInterfaceMessage message)
        {
            base.ReceiveMessage(message);

            if (message is ValidationUpdateMessage msg)
            {
                Validation = new Regex(msg.ValidationString, RegexOptions.Compiled);
            }
        }

        public void SendConfiguration(Dictionary<string, string> config)
        {
            SendMessage(new ConfigurationUpdatedMessage(config));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing && _menu != null)
            {
                _menu.OnClose -= Close;
                _menu.Close();
            }
        }
    }
}
