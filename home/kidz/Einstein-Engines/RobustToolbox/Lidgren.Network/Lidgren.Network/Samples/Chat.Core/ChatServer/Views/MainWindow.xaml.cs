using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ChatServer.ViewModels;
using ReactiveUI;
using SamplesCommon.Core;

namespace ChatServer.Views
{
    public class MainWindow : Window
    {
        private NetPeerSettingsWindow _settings;

        public MainWindow()
        {
            InitializeComponent();

            this.FindControl<Button>("SettingsButton").Command = ReactiveCommand.Create(SettingsPressed);
        }

        private void SettingsPressed()
        {
            if (!(DataContext is MainWindowViewModel vm))
            {
                return;
            }

            if (_settings == null)
            {
                _settings = new NetPeerSettingsWindow(vm.NetPeer)
                {
                    Icon = Icon,
                    Title = "Chat server settings"
                };
                _settings.Closed += (sender, args) => _settings = null;
                _settings.Show();
            }
            else
            {
                _settings.Close();
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}