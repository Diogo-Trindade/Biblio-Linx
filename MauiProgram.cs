using Microsoft.Extensions.Logging;
using BiblioLinx.Services;
using BiblioLinx.ViewModels;
using Syncfusion.Maui.Core.Hosting;
using Microsoft.Maui.LifecycleEvents; // <-- ADICIONADO PARA O HACK DA JANELA

namespace BiblioLinx;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureSyncfusionCore()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("SEGOEICONS.TTF", "FluentIcons");
            })
            // ====================================================================
            // HACK DEFINITIVO E SEGURO PARA FORÇAR O MODO CLARO NO WINDOWS (WINUI3)
            // ====================================================================
            .ConfigureLifecycleEvents(events =>
            {
#if WINDOWS
                events.AddWindows(windowsLifecycleBuilder =>
                {
                    windowsLifecycleBuilder.OnWindowCreated(window =>
                    {
                        if (window.Content is Microsoft.UI.Xaml.FrameworkElement frameworkElement)
                        {
                            // Esta linha diz ao Windows: "Este programa é modo claro, ponto final."
                            frameworkElement.RequestedTheme = Microsoft.UI.Xaml.ElementTheme.Light;
                        }
                    });
                });
#endif
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<IGroqApiService, GroqApiService>();
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<MainPage>();

        return builder.Build();
    }
}