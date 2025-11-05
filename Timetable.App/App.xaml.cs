using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace Timetable.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += App_DispatcherUnhandledException;
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        // Log the exception
        var exceptionLogPath = Path.Combine(AppContext.BaseDirectory, "error_log.txt");
        File.WriteAllText(exceptionLogPath, $"{e.Exception}");

        // Prevent default unhandled exception processing
        e.Handled = true;

        // Optionally, show a message to the user
        MessageBox.Show($"An unexpected error occurred. Please check the log file at {exceptionLogPath}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

        // Shutdown the application
        Shutdown();
    }
}

