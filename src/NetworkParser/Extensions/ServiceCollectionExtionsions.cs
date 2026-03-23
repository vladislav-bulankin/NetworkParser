using NetworkParser.Core.Abstractions.Connection;
using NetworkParser.Core.Abstractions.Parsers;
using NetworkParser.Core.Connection;
using NetworkParser.Core.Parsers;
using NetworkParser.UI.ViewModels;
using NetworkParser.ViewModels;

namespace NetworkParser.UI.Extensions;

internal static class ServiceCollectionExtionsions {
    internal static void InjectAll (this IServiceCollection services) {
        services.AddSingleton<ITlsParser, TlsParser>();
        services.AddSingleton<IPacketParser, PacketParser>();
        services.AddSingleton<INetworkParserController, NetworkParserController>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<PacketListViewModel>();
        services.AddSingleton<PacketDetailsViewModel>();
        services.AddSingleton<HexViewerViewModel>();
        services.AddSingleton<PacketCrafterViewModel>();
        services.AddSingleton<NetworkParser.UI.Views.MainPage>();
    }
}
