using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PickMeWhatToListen.Application;
using PickMeWhatToListen.Infrastructure;
using PickMeWhatToListen.Wpf.ViewModels;
#if DEBUG
using XamlMcp.Wpf;
#endif

namespace PickMeWhatToListen.Wpf;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
/// <remarks>
/// Must fully-qualify <see cref="System.Windows.Application"/>: the sibling
/// PickMeWhatToListen.Application project namespace shadows the unqualified
/// "Application" name for every type in this project (see .cursor/rules/mvvm-wpf.mdc).
/// </remarks>
public partial class App : System.Windows.Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
#if DEBUG
        this.AttachXamlMcp();
#endif
        base.OnStartup(e);

        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddInfrastructure();
        builder.Services.AddTransient<ArtistCatalogService>();
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<MainWindow>();

        _host = builder.Build();
        await _host.StartAsync();

        await DatabaseMigrator.MigrateAsync(_host.Services);

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        base.OnExit(e);
    }
}
