using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ChatServer.ViewModels;
using ChatServer.Views;

namespace ChatServer
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var viewModel = new MainWindowViewModel();
                desktop.MainWindow = new MainWindow
                {
                    DataContext = viewModel,
                };

                viewModel.Initialized();
                desktop.Exit += (sender, args) =>
                {
                    viewModel.Exiting();
                };
                desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}