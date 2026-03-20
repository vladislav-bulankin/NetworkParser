using NetworkParser.UI.Views;
using Uno.Resizetizer;
using NetworkParser.UI.Extensions;

namespace NetworkParser;

public partial class App : Application {
    /// <summary>
    /// Initializes the singleton application object. This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App () {
        this.InitializeComponent();

    }

    public Window? MainWindow { get; private set; }
    protected IHost? Host { get; private set; }

    protected override void OnLaunched (LaunchActivatedEventArgs args) {
        var builder = this.CreateBuilder(args)
        .Configure(host => host
#if DEBUG
            .UseEnvironment(Environments.Development)
#endif
            .UseLogging(configure: (context, logBuilder) => {
                logBuilder
                    .SetMinimumLevel(
                        context.HostingEnvironment.IsDevelopment() ?
                            LogLevel.Information :
                            LogLevel.Warning)
                    .CoreLogLevel(LogLevel.Warning);
            }, enableUnoLogging: true)
            .UseConfiguration(configure: configBuilder =>
                configBuilder
                    .EmbeddedSource<App>()
                    .Section<AppConfig>()
            )
            .ConfigureServices((context, services) => {
                services.InjectAll(); 
            })
        );

        MainWindow = builder.Window;

#if DEBUG
        MainWindow.UseStudio();
#endif

        Host = builder.Build();

        var mainPage = Host.Services.GetRequiredService<MainPage>();

        MainWindow.Content = mainPage;
        MainWindow.Activate();
    }
}
