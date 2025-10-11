using ListaComprasApp.Views;
using ListaComprasApp.ViewModels;
using ListaComprasApp.Services;

namespace ListaComprasApp;

public class MainTabbedPage : TabbedPage
{
    public MainTabbedPage()
    {
        // Configuração visual das abas
        BarBackgroundColor = Colors.Purple;
        BarTextColor = Colors.White;
        SelectedTabColor = Colors.White;
        UnselectedTabColor = Color.FromRgba(255, 255, 255, 150);

        // Criar DatabaseService
        var databaseService = new DatabaseService();

        // Aba 1: Lista de Compras (página existente)
        var produtosPadraoPage = new NavigationPage(new ProdutosPadraoPage(new ProdutosPadraoViewModel(databaseService)))
        {
            Title = "🛒 Lista",
            IconImageSource = "cart_icon.png", // Opcional: adicione um ícone se tiver
            BarBackgroundColor = Colors.Purple,
            BarTextColor = Colors.White
        };

        // Aba 2: Relatórios (nova página)
        var dashboardPage = new NavigationPage(new DashboardPage())
        {
            Title = "📊 Relatórios",
            IconImageSource = "chart_icon.png", // Opcional: adicione um ícone se tiver
            BarBackgroundColor = Colors.Purple,
            BarTextColor = Colors.White
        };

        // Adicionar abas
        Children.Add(produtosPadraoPage);
        Children.Add(dashboardPage);
    }
}