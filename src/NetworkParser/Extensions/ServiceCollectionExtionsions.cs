using System;
using System.Collections.Generic;
using System.Text;
using NetworkParser.Core.Abstractions.Connection;
using NetworkParser.Core.Connection;
using NetworkParser.ViewModels;

namespace NetworkParser.UI.Extensions;

internal static class ServiceCollectionExtionsions {
    internal static void InjectAll (this IServiceCollection services) {
        services.AddSingleton<INetworkParserController, NetworkParserController>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<PacketListViewModel>();
        services.AddSingleton<PacketDetailsViewModel>();
        services.AddSingleton<HexViewerViewModel>();
        services.AddSingleton<NetworkParser.UI.Views.MainPage>();
    }
}
