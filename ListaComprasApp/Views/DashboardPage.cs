using ListaComprasApp.Services;
using ListaComprasApp.Models;
using System.Globalization;

namespace ListaComprasApp.Views;

public class DashboardPage : ContentPage
{
    private DateTime _mesAtual = DateTime.Now;
    private Label _mesLabel;
    private Button _voltarMesButton;
    private Button _avancarMesButton;
    private StackLayout _conteudoStack;

    public DashboardPage()
    {
        Title = "Relatórios";
        CriarInterface();
        CarregarDados();
    }

    private void CriarInterface()
    {
        var mainGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto }, // Navegação de mês
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) } // Conteúdo
            },
            BackgroundColor = Color.FromArgb("#F5F5F5")
        };

        // ===== NAVEGAÇÃO DE MÊS =====
        var navegacaoFrame = new Frame
        {
            BackgroundColor = Colors.Purple,
            Padding = 15,
            CornerRadius = 0,
            HasShadow = true
        };

        var navegacaoStack = new StackLayout
        {
            Orientation = StackOrientation.Horizontal,
            HorizontalOptions = LayoutOptions.Center,
            Spacing = 20
        };

        _voltarMesButton = new Button
        {
            Text = "◀",
            FontSize = 18,
            BackgroundColor = Colors.White,
            TextColor = Colors.Purple,
            WidthRequest = 40,
            HeightRequest = 40,
            CornerRadius = 20,
            Padding = 0
        };
        _voltarMesButton.Clicked += OnVoltarMesClicked;

        _mesLabel = new Label
        {
            Text = ObterNomeMes(_mesAtual),
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            WidthRequest = 200,
            HorizontalTextAlignment = TextAlignment.Center
        };

        _avancarMesButton = new Button
        {
            Text = "▶",
            FontSize = 18,
            BackgroundColor = Colors.White,
            TextColor = Colors.Purple,
            WidthRequest = 40,
            HeightRequest = 40,
            CornerRadius = 20,
            Padding = 0
        };
        _avancarMesButton.Clicked += OnAvancarMesClicked;

        navegacaoStack.Children.Add(_voltarMesButton);
        navegacaoStack.Children.Add(_mesLabel);
        navegacaoStack.Children.Add(_avancarMesButton);

        navegacaoFrame.Content = navegacaoStack;
        mainGrid.Children.Add(navegacaoFrame);
        Grid.SetRow(navegacaoFrame, 0);

        // ===== CONTEÚDO =====
        var scrollView = new ScrollView();
        _conteudoStack = new StackLayout
        {
            Padding = 20,
            Spacing = 15
        };

        scrollView.Content = _conteudoStack;
        mainGrid.Children.Add(scrollView);
        Grid.SetRow(scrollView, 1);

        Content = mainGrid;
        AtualizarBotoesNavegacao();
    }

    private void CarregarDados()
    {
        _conteudoStack.Children.Clear();

        // Usar HistoricoMensal existente para pegar dados
        var historicoMes = HistoricoMensal.ObterHistoricoMes(_mesAtual);

        if (historicoMes == null || historicoMes.Count == 0)
        {
            MostrarMensagemVazio();
            return;
        }

        // Calcular totais
        decimal totalMes = 0;
        var totalPorCategoria = new Dictionary<Categoria, decimal>();

        var todosProdutos = ProdutosPadraoService.ObterProdutosAtivos();

        foreach (var item in historicoMes)
        {
            var produto = todosProdutos?.FirstOrDefault(p => p.Nome == item.Key);
            if (produto.HasValue)
            {
                var (quantidade, valor) = item.Value;
                var subtotal = quantidade * valor;
                totalMes += subtotal;

                if (!totalPorCategoria.ContainsKey(produto.Value.Categoria))
                    totalPorCategoria[produto.Value.Categoria] = 0;

                totalPorCategoria[produto.Value.Categoria] += subtotal;
            }
        }

        // Card 1: Total do Mês
        CriarCardTotalMes(totalMes);

        // Card 2: Comparação com mês anterior
        CriarCardComparacao(totalMes);

        // Card 3: Top 5 Categorias
        CriarCardTopCategorias(totalPorCategoria);
    }

    private void CriarCardTotalMes(decimal total)
    {
        var card = new Frame
        {
            BackgroundColor = Colors.White,
            CornerRadius = 15,
            HasShadow = true,
            Padding = 20
        };

        var stack = new StackLayout { Spacing = 10 };

        var titulo = new Label
        {
            Text = "💰 TOTAL DO MÊS",
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.Gray
        };

        var valor = new Label
        {
            Text = string.Format(CultureInfo.GetCultureInfo("pt-BR"), "R$ {0:N2}", total),
            FontSize = 32,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.Purple
        };

        stack.Children.Add(titulo);
        stack.Children.Add(valor);
        card.Content = stack;

        _conteudoStack.Children.Add(card);
    }

    private void CriarCardComparacao(decimal totalAtual)
    {
        var mesAnterior = _mesAtual.AddMonths(-1);
        var historicoMesAnterior = HistoricoMensal.ObterHistoricoMes(mesAnterior);

        decimal totalAnterior = 0;

        if (historicoMesAnterior != null)
        {
            var todosProdutos = ProdutosPadraoService.ObterProdutosAtivos();
            foreach (var item in historicoMesAnterior)
            {
                var (quantidade, valor) = item.Value;
                totalAnterior += quantidade * valor;
            }
        }

        var diferenca = totalAtual - totalAnterior;
        var percentual = totalAnterior > 0 ? (diferenca / totalAnterior) * 100 : 0;
        var emoji = diferenca > 0 ? "↑" : diferenca < 0 ? "↓" : "→";
        var cor = diferenca > 0 ? Colors.Red : diferenca < 0 ? Colors.Green : Colors.Gray;

        var card = new Frame
        {
            BackgroundColor = Colors.White,
            CornerRadius = 15,
            HasShadow = true,
            Padding = 20
        };

        var stack = new StackLayout { Spacing = 10 };

        var titulo = new Label
        {
            Text = "📈 COMPARAÇÃO COM MÊS ANTERIOR",
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.Gray
        };

        var comparacaoStack = new StackLayout
        {
            Orientation = StackOrientation.Horizontal,
            Spacing = 10
        };

        var emojiLabel = new Label
        {
            Text = emoji,
            FontSize = 32,
            TextColor = cor,
            VerticalOptions = LayoutOptions.Center
        };

        var infoStack = new StackLayout { Spacing = 5 };

        var diferencaLabel = new Label
        {
            Text = string.Format(CultureInfo.GetCultureInfo("pt-BR"), "R$ {0:N2}", Math.Abs(diferenca)),
            FontSize = 24,
            FontAttributes = FontAttributes.Bold,
            TextColor = cor
        };

        var percentualLabel = new Label
        {
            Text = string.Format(CultureInfo.GetCultureInfo("pt-BR"), "{0:N1}%", Math.Abs(percentual)),
            FontSize = 14,
            TextColor = Colors.Gray
        };

        infoStack.Children.Add(diferencaLabel);
        infoStack.Children.Add(percentualLabel);

        comparacaoStack.Children.Add(emojiLabel);
        comparacaoStack.Children.Add(infoStack);

        stack.Children.Add(titulo);
        stack.Children.Add(comparacaoStack);
        card.Content = stack;

        _conteudoStack.Children.Add(card);
    }

    private void CriarCardTopCategorias(Dictionary<Categoria, decimal> totalPorCategoria)
    {
        if (totalPorCategoria.Count == 0)
            return;

        var card = new Frame
        {
            BackgroundColor = Colors.White,
            CornerRadius = 15,
            HasShadow = true,
            Padding = 20
        };

        var stack = new StackLayout { Spacing = 15 };

        var titulo = new Label
        {
            Text = "🏆 TOP 5 CATEGORIAS",
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.Gray
        };

        stack.Children.Add(titulo);

        var totalGeral = totalPorCategoria.Values.Sum();
        var top5 = totalPorCategoria.OrderByDescending(x => x.Value).Take(5);

        foreach (var item in top5)
        {
            var categoriaStack = new StackLayout { Spacing = 5 };

            var headerStack = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = 10
            };

            var nomeCategoria = new Label
            {
                Text = ObterNomeCategoria(item.Key),
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.StartAndExpand
            };

            var valorCategoria = new Label
            {
                Text = string.Format(CultureInfo.GetCultureInfo("pt-BR"), "R$ {0:N2}", item.Value),
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Purple
            };

            headerStack.Children.Add(nomeCategoria);
            headerStack.Children.Add(valorCategoria);

            var percentual = (item.Value / totalGeral) * 100;

            var barraGrid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };

            var progressBarContainer = new Frame
            {
                BackgroundColor = Color.FromArgb("#E0E0E0"),
                CornerRadius = 4,
                Padding = 0,
                HasShadow = false,
                HeightRequest = 8,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };

            var progressBar = new BoxView
            {
                BackgroundColor = ObterCorCategoria(item.Key),
                WidthRequest = (double)percentual,
                HeightRequest = 8,
                HorizontalOptions = LayoutOptions.Start
            };

            progressBarContainer.Content = progressBar;
            barraGrid.Children.Add(progressBarContainer);
            Grid.SetColumn(progressBarContainer, 0);

            var percentualLabel = new Label
            {
                Text = $"{percentual:N0}%",
                FontSize = 12,
                TextColor = Colors.Gray,
                VerticalOptions = LayoutOptions.Center,
                WidthRequest = 40
            };
            barraGrid.Children.Add(percentualLabel);
            Grid.SetColumn(percentualLabel, 1);

            categoriaStack.Children.Add(headerStack);
            categoriaStack.Children.Add(barraGrid);

            stack.Children.Add(categoriaStack);
        }

        card.Content = stack;
        _conteudoStack.Children.Add(card);
    }

    private void MostrarMensagemVazio()
    {
        var mensagemStack = new StackLayout
        {
            VerticalOptions = LayoutOptions.CenterAndExpand,
            HorizontalOptions = LayoutOptions.Center,
            Spacing = 20,
            Padding = 40
        };

        var icone = new Label
        {
            Text = "📊",
            FontSize = 80,
            HorizontalOptions = LayoutOptions.Center
        };

        var texto = new Label
        {
            Text = "Nenhuma compra finalizada neste mês",
            FontSize = 18,
            TextColor = Colors.Gray,
            HorizontalTextAlignment = TextAlignment.Center
        };

        var subtexto = new Label
        {
            Text = "Finalize compras na aba Lista para ver relatórios aqui",
            FontSize = 14,
            TextColor = Colors.LightGray,
            HorizontalTextAlignment = TextAlignment.Center
        };

        mensagemStack.Children.Add(icone);
        mensagemStack.Children.Add(texto);
        mensagemStack.Children.Add(subtexto);

        _conteudoStack.Children.Add(mensagemStack);
    }

    private string ObterNomeCategoria(Categoria categoria)
    {
        return categoria switch
        {
            Categoria.FrutasVerduras => "🍎 Frutas e Verduras",
            Categoria.Carnes => "🥩 Carnes",
            Categoria.Laticinios => "🥛 Laticínios",
            Categoria.Bebidas => "🥤 Bebidas",
            Categoria.Limpeza => "🧹 Limpeza",
            Categoria.Padaria => "🍞 Padaria",
            Categoria.Congelados => "❄️ Congelados",
            Categoria.Higiene => "🧼 Higiene",
            Categoria.AlimentosBasicos => "🌾 Alimentos Básicos",
            Categoria.EnlatadosConservas => "🥫 Enlatados",
            Categoria.BiscoitosSnacks => "🍪 Biscoitos",
            Categoria.DocesSobremesas => "🍰 Doces",
            Categoria.TemperoCondimentos => "🧂 Temperos",
            Categoria.CafesChas => "☕ Cafés e Chás",
            Categoria.BebeInfantil => "👶 Bebê",
            Categoria.PetShop => "🐾 Pet Shop",
            Categoria.UtilidadesDomesticas => "🏠 Utilidades",
            Categoria.Descartaveis => "🗑️ Descartáveis",
            Categoria.HortifrutiEspeciais => "🥗 Hortifruti",
            Categoria.ComidasProntas => "🍱 Comidas Prontas",
            Categoria.Farmacia => "💊 Farmácia",
            Categoria.FestasDecoracao => "🎉 Festas",
            _ => categoria.ToString()
        };
    }

    private string ObterNomeMes(DateTime data)
    {
        var cultura = new CultureInfo("pt-BR");
        return cultura.TextInfo.ToTitleCase(data.ToString("MMMM/yyyy", cultura));
    }

    private Color ObterCorCategoria(Categoria categoria)
    {
        return categoria switch
        {
            Categoria.FrutasVerduras => Color.FromArgb("#4CAF50"),
            Categoria.Carnes => Color.FromArgb("#F44336"),
            Categoria.Laticinios => Color.FromArgb("#2196F3"),
            Categoria.Bebidas => Color.FromArgb("#FF9800"),
            Categoria.Limpeza => Color.FromArgb("#9C27B0"),
            Categoria.Padaria => Color.FromArgb("#FF5722"),
            Categoria.Congelados => Color.FromArgb("#00BCD4"),
            Categoria.Higiene => Color.FromArgb("#E91E63"),
            Categoria.AlimentosBasicos => Color.FromArgb("#795548"),
            Categoria.EnlatadosConservas => Color.FromArgb("#607D8B"),
            Categoria.BiscoitosSnacks => Color.FromArgb("#FFC107"),
            Categoria.DocesSobremesas => Color.FromArgb("#E91E63"),
            Categoria.TemperoCondimentos => Color.FromArgb("#8BC34A"),
            Categoria.CafesChas => Color.FromArgb("#5D4037"),
            Categoria.BebeInfantil => Color.FromArgb("#FF4081"),
            Categoria.PetShop => Color.FromArgb("#FF6F00"),
            Categoria.UtilidadesDomesticas => Color.FromArgb("#455A64"),
            Categoria.Descartaveis => Color.FromArgb("#9E9E9E"),
            Categoria.HortifrutiEspeciais => Color.FromArgb("#66BB6A"),
            Categoria.ComidasProntas => Color.FromArgb("#FFA726"),
            Categoria.Farmacia => Color.FromArgb("#EF5350"),
            Categoria.FestasDecoracao => Color.FromArgb("#AB47BC"),
            _ => Colors.Gray
        };
    }

    private void AtualizarBotoesNavegacao()
    {
        var mesAtualReal = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var mesSelecionado = new DateTime(_mesAtual.Year, _mesAtual.Month, 1);

        _avancarMesButton.IsEnabled = mesSelecionado < mesAtualReal;
        _avancarMesButton.Opacity = _avancarMesButton.IsEnabled ? 1.0 : 0.5;
    }

    private void OnVoltarMesClicked(object sender, EventArgs e)
    {
        _mesAtual = _mesAtual.AddMonths(-1);
        _mesLabel.Text = ObterNomeMes(_mesAtual);
        AtualizarBotoesNavegacao();
        CarregarDados();
    }

    private void OnAvancarMesClicked(object sender, EventArgs e)
    {
        _mesAtual = _mesAtual.AddMonths(1);
        _mesLabel.Text = ObterNomeMes(_mesAtual);
        AtualizarBotoesNavegacao();
        CarregarDados();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        CarregarDados();
    }
}