using NetworkParser.UI.Extensions;
using NetworkParser.UI.Views;

namespace NetworkParser;

public partial class App : Application {

    public App () {
        this.InitializeComponent();

    }

    public Window? MainWindow { get; private set; }
    protected IHost? Host { get; private set; }

    protected override void OnLaunched (LaunchActivatedEventArgs args) {
        if (!IsPcapInstalled()) {
            ShowDriverError();
            return; // Прекращаем запуск
        }
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
        MainWindow.Title = "Black Sniffer 1.0";
        MainWindow.AppWindow.SetIcon("Assets/AppIcon.ico");

#if DEBUG
        MainWindow.UseStudio();
#endif

        Host = builder.Build();

        var mainPage = Host.Services.GetRequiredService<MainPage>();

        MainWindow.Content = mainPage;
        MainWindow.Activate();
    }

    private bool IsPcapInstalled () {
        try {
            // Пытаемся получить список устройств
            return SharpPcap.LibPcap.LibPcapLiveDeviceList.Instance.Count > 0;
        } catch {
            return false;
        }
    }

    private void ShowDriverError () {
        var visual = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        visual.Children.Add(new TextBlock {
            Text = "Ошибка: WinPcap или Npcap не найден!",
            HorizontalAlignment = HorizontalAlignment.Center,
            FontSize = 20
        });

        // Если окно уже создано билдером:
        if (MainWindow != null) {
            MainWindow.Content = visual;
            MainWindow.Activate();
        }
    }
}
