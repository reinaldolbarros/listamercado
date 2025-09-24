using ListaComprasApp.Services;
using ListaComprasApp.ViewModels;
using ListaComprasApp.Views;

namespace ListaComprasApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Registrar serviços
        builder.Services.AddSingleton<DatabaseService>();

        // Registrar ViewModels
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddTransient<ListaDetalhesViewModel>();

        // Registrar Views
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddTransient<ListaDetalhesPage>();

        // Registrar ViewModels
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddTransient<ListaDetalhesViewModel>();
        builder.Services.AddTransient<ProdutosPadraoViewModel>(); // ADICIONAR ESTA LINHA

        // Registrar Views
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddTransient<ListaDetalhesPage>();
        builder.Services.AddTransient<ProdutosPadraoPage>(); // ADICIONAR ESTA LINHA

        return builder.Build();
    }
}