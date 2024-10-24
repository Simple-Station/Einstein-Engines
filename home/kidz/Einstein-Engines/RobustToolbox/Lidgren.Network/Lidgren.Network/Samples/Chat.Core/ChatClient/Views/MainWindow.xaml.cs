using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using ChatClient.ViewModels;
using ReactiveUI;
using SamplesCommon.Core;

namespace ChatClient.Views
{
    public class MainWindow : Window
    {
        private NetPeerSettingsWindow _settings;

        public MainWindow()
        {
            InitializeComponent();

            this.FindControl<Button>("SettingsButton").Command = ReactiveCommand.Create(SettingsPressed);
            this.FindControl<TextBox>("MessageBox").KeyDown += (sender, args) =>
            {
                if (args.Key == Key.Enter && DataContext is MainWindowViewModel vm)
                {
                    vm.SendPressed();
                }
            };
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
                    Title = "Chat client settings"
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