using ListaComprasApp.ViewModels;
using ListaComprasApp.Services;
using ListaComprasApp.Models;
using System.Globalization;

namespace ListaComprasApp.Views;

public partial class ProdutosPadraoPage : ContentPage
{
    private readonly ProdutosPadraoViewModel _viewModel;
    private readonly Dictionary<string, int> _quantidades = new();
    private readonly Dictionary<string, Entry> _valoresUnitarios = new();
    private readonly Dictionary<string, CheckBox> _checkboxes = new();
    private Dictionary<string, bool> _checkboxStates = new();
    private Label _totalLabel;
    private Label _totalCheckadosLabel;
    private ScrollView _scrollView;

    // Dicionários para rastrear elementos visuais
    private readonly Dictionary<string, View> _produtoContainers = new();
    private readonly Dictionary<Categoria, (Label CategoriaLabel, StackLayout ProdutosStack)> _categoriasUI = new();
    private StackLayout _mainStackLayout;

    public ProdutosPadraoPage(ProdutosPadraoViewModel viewModel)
    {
        _viewModel = viewModel;
        BindingContext = viewModel;
        Title = "Lista de Compras";
        CreateUI();
        LoadProducts();
    }

    private void CreateUI()
    {
        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("pt-BR");
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("pt-BR");

        var mainGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto }
            }
        };

        _scrollView = new ScrollView();
        _mainStackLayout = new StackLayout { Padding = 20 };

        var adicionarCategoriaButton = new Button
        {
            Text = "+ Novo produto",
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            BackgroundColor = Colors.Purple,
            TextColor = Colors.White,
            CornerRadius = 8,
            Margin = new Thickness(0, 0, 0, 0),
            Padding = new Thickness(0, -1, 0, 0),
            HeightRequest = 40,
            WidthRequest = 140,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        adicionarCategoriaButton.Clicked += OnAdicionarCategoriaClicked;
        _mainStackLayout.Children.Add(adicionarCategoriaButton);

        var produtos = ProdutosPadraoService.ObterProdutosAtivos();
        var categorias = produtos.GroupBy(p => p.Categoria);

        foreach (var categoria in categorias)
        {
            CriarSecaoCategoria(categoria.Key, categoria.ToList());
        }

        var botoesStack = new StackLayout
        {
            Orientation = StackOrientation.Horizontal,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 20, 0, 0)
        };

        var editarButton = new Button
        {
            Text = "Editar Lista",
            FontSize = 16,
            BackgroundColor = Colors.Orange,
            TextColor = Colors.White,
            Margin = new Thickness(0, 0, 10, 0),
            CornerRadius = 8
        };
        editarButton.Clicked += OnEditarListaClicked;

        var finalizarButton = new Button
        {
            Text = "Finalizar Compra",
            FontSize = 16,
            BackgroundColor = Colors.Green,
            TextColor = Colors.White,
            CornerRadius = 8
        };
        finalizarButton.Clicked += OnFinalizarCompraClicked;

        botoesStack.Children.Add(editarButton);
        botoesStack.Children.Add(finalizarButton);
        _mainStackLayout.Children.Add(botoesStack);

        _scrollView.Content = _mainStackLayout;
        mainGrid.Children.Add(_scrollView);
        Grid.SetRow(_scrollView, 0);

        var totaisFrame = new Frame
        {
            BackgroundColor = Colors.LightGray,
            Padding = new Thickness(10, 5),
            Margin = 0,
            BorderColor = Colors.Transparent,
            CornerRadius = 0,
            HasShadow = false
        };

        var totaisGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            }
        };

        _totalLabel = new Label
        {
            Text = string.Format(CultureInfo.GetCultureInfo("pt-BR"), "TOTAL PREVISTO:\nR$ {0:N2}", produtos.Sum(p => p.PrecoMedio)),
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.Green,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        totaisGrid.Children.Add(_totalLabel);
        Grid.SetColumn(_totalLabel, 0);

        _totalCheckadosLabel = new Label
        {
            Text = "TOTAL COMPRADO:\nR$ 0,00",
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.Blue,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        totaisGrid.Children.Add(_totalCheckadosLabel);
        Grid.SetColumn(_totalCheckadosLabel, 1);

        TotaisManager.TotalChanged += (newText) => _totalLabel.Text = newText;
        TotaisManager.TotalCompradoChanged += (newText) => _totalCheckadosLabel.Text = newText;

        totaisFrame.Content = totaisGrid;
        mainGrid.Children.Add(totaisFrame);
        Grid.SetRow(totaisFrame, 1);

        Content = mainGrid;

        AtualizarTotalGeral();
        AtualizarTotalCheckados();
    }

    private void CriarSecaoCategoria(Categoria categoria, List<(string Nome, UnidadeMedida Unidade, Categoria Categoria, string Icone, decimal PrecoMedio)> produtos)
    {
        var categoriaTexto = ObterTextoCategoria(categoria);

        var categoriaLabel = new Label
        {
            Text = categoriaTexto,
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.Purple,
            Margin = new Thickness(0, 15, 0, 5)
        };
        _mainStackLayout.Children.Add(categoriaLabel);

        var produtosStack = new StackLayout { Spacing = 2 };
        _mainStackLayout.Children.Add(produtosStack);

        _categoriasUI[categoria] = (categoriaLabel, produtosStack);

        foreach (var produto in produtos)
        {
            AdicionarProdutoNaUI(produto, produtosStack);
        }
    }

    private void AdicionarProdutoNaUI((string Nome, UnidadeMedida Unidade, Categoria Categoria, string Icone, decimal PrecoMedio) produto, StackLayout containerStack)
    {
        var itemContainer = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            Margin = new Thickness(0, 2)
        };

        var itemFrame = new Frame
        {
            BackgroundColor = Colors.White,
            BorderColor = Colors.LightGray,
            CornerRadius = 8,
            Padding = new Thickness(5, 5, 4, 3),
            Margin = new Thickness(0),
            HeightRequest = 62
        };

        var itemGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(4, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(90, GridUnitType.Absolute) },
                new ColumnDefinition { Width = new GridLength(75, GridUnitType.Absolute) }
            }
        };

        var checkbox = new CheckBox
        {
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(-10, 0, 0, 0)
        };

        if (_checkboxStates.ContainsKey(produto.Nome))
        {
            checkbox.IsChecked = _checkboxStates[produto.Nome];
        }

        _checkboxes[produto.Nome] = checkbox;
        checkbox.CheckedChanged += (s, e) =>
        {
            _checkboxStates[produto.Nome] = checkbox.IsChecked;
            AtualizarTotalCheckados();
        };
        itemGrid.Children.Add(checkbox);
        Grid.SetColumn(checkbox, 0);

        var iconeLabel = new Label
        {
            Text = produto.Icone,
            FontSize = 24,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(-2, 0, 5, 0)
        };
        itemGrid.Children.Add(iconeLabel);
        Grid.SetColumn(iconeLabel, 1);

        if (!_quantidades.ContainsKey(produto.Nome))
        {
            _quantidades[produto.Nome] = 1;
        }

        var detalhesStack = new StackLayout
        {
            VerticalOptions = LayoutOptions.Start,
            HorizontalOptions = LayoutOptions.Start,
            Margin = new Thickness(5, 0, 10, 0),
            Spacing = 2
        };

        var nomeUpper = produto.Nome.ToUpper();
        var temEspacos = nomeUpper.Contains(' ');

        var nomeLabel = new Label
        {
            Text = nomeUpper,
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            LineBreakMode = temEspacos ? LineBreakMode.WordWrap : LineBreakMode.NoWrap,
            MaxLines = temEspacos ? 2 : 1,
            VerticalOptions = LayoutOptions.Start,
            HorizontalOptions = LayoutOptions.Start,
            WidthRequest = 120
        };

        if (nomeUpper.Length > 8) nomeLabel.FontSize = 12;
        if (nomeUpper.Length > 12) nomeLabel.FontSize = 10;
        if (nomeUpper.Length > 18) nomeLabel.FontSize = 9;
        detalhesStack.Children.Add(nomeLabel);

        var unidadeTexto = produto.Unidade switch
        {
            UnidadeMedida.Kilo => "kg",
            UnidadeMedida.Grama => "g",
            UnidadeMedida.Litro => "L",
            UnidadeMedida.Unidade => "un",
            UnidadeMedida.Pacote => "pct",
            UnidadeMedida.Caixa => "cx",
            _ => "un"
        };

        var unidadeQuantidadeStack = new StackLayout
        {
            Orientation = StackOrientation.Horizontal,
            Spacing = 5
        };

        var detalhesLabel = new Label
        {
            Text = unidadeTexto,
            FontSize = 10,
            TextColor = Colors.Gray,
            VerticalOptions = LayoutOptions.Center
        };
        unidadeQuantidadeStack.Children.Add(detalhesLabel);

        var quantidadeLabel = new Label
        {
            Text = _quantidades[produto.Nome].ToString(),
            FontSize = 10,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.Gray,
            VerticalOptions = LayoutOptions.Center
        };
        unidadeQuantidadeStack.Children.Add(quantidadeLabel);

        detalhesStack.Children.Add(unidadeQuantidadeStack);
        itemGrid.Children.Add(detalhesStack);
        Grid.SetColumn(detalhesStack, 2);

        var precoLabel = new Label
        {
            Text = string.Format(CultureInfo.GetCultureInfo("pt-BR"), "R$ {0:N2}", produto.PrecoMedio * _quantidades[produto.Nome]),
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.Green,
            VerticalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.End,
            Margin = new Thickness(0, 0, 0, 0),
        };

        var valorEntry = new Entry
        {
            Placeholder = "0,00",
            Text = produto.PrecoMedio.ToString("N2", CultureInfo.GetCultureInfo("pt-BR")),
            FontSize = 12,
            Keyboard = Keyboard.Numeric,
            WidthRequest = 65,
            HeightRequest = 50,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, -8, 0, 0)
        };

        _valoresUnitarios[produto.Nome] = valorEntry;

        var produtoAtual = produto;
        var isUserInput = false;

        valorEntry.Focused += (s, e) =>
        {
            var entry = s as Entry;
            if (entry != null)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    entry.CursorPosition = 0;
                    entry.SelectionLength = entry.Text?.Length ?? 0;
                });
            }
        };

        valorEntry.TextChanged += (s, e) =>
        {
            var entry = s as Entry;
            if (entry != null)
            {
                if (isUserInput) return;
                isUserInput = true;

                var numericText = new string(entry.Text.Where(char.IsDigit).ToArray());

                if (!string.IsNullOrEmpty(numericText))
                {
                    if (long.TryParse(numericText, out long value))
                    {
                        var formattedValue = (decimal)value / 100;
                        var expectedText = formattedValue.ToString("N2", CultureInfo.GetCultureInfo("pt-BR"));

                        entry.Text = expectedText;
                        entry.CursorPosition = expectedText.Length;

                        var quantidade = _quantidades[produtoAtual.Nome];
                        precoLabel.Text = string.Format(CultureInfo.GetCultureInfo("pt-BR"), "R$ {0:N2}", formattedValue * quantidade);
                        AtualizarTotalGeral();
                    }
                }
                else
                {
                    entry.Text = "0,00";
                    precoLabel.Text = "R$ 0,00";
                    AtualizarTotalGeral();
                }

                isUserInput = false;
            }
        };

        var quantidadeContainer = new StackLayout
        {
            Orientation = StackOrientation.Horizontal,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Start,
            Spacing = 5,
            Padding = new Thickness(0, 0, 0, 0)
        };

        quantidadeContainer.Children.Add(valorEntry);

        var botoesPlusMinusStack = new StackLayout
        {
            Orientation = StackOrientation.Vertical,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Start,
            Spacing = 2,
            Margin = new Thickness(0, 0, 0, 0)
        };

        var aumentarButton = new Button
        {
            Text = "+",
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            BackgroundColor = Colors.LightGreen,
            TextColor = Colors.White,
            WidthRequest = 24,
            HeightRequest = 24,
            CornerRadius = 10,
            Padding = 0
        };

        var diminuirButton = new Button
        {
            Text = "-",
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            BackgroundColor = Colors.LightCoral,
            TextColor = Colors.White,
            WidthRequest = 24,
            HeightRequest = 24,
            CornerRadius = 10,
            Padding = 0
        };

        decimal ObterValorUnitario()
        {
            if (_valoresUnitarios.ContainsKey(produtoAtual.Nome))
            {
                var entry = _valoresUnitarios[produtoAtual.Nome];
                if (decimal.TryParse(entry.Text, out decimal valor))
                    return valor;
            }
            return 0;
        }

        diminuirButton.Clicked += (s, e) =>
        {
            if (_quantidades[produtoAtual.Nome] > 1)
            {
                _quantidades[produtoAtual.Nome]--;
                quantidadeLabel.Text = _quantidades[produtoAtual.Nome].ToString();
                var valorUnitario = ObterValorUnitario();
                precoLabel.Text = string.Format(CultureInfo.GetCultureInfo("pt-BR"), "R$ {0:N2}", valorUnitario * _quantidades[produtoAtual.Nome]);
                AtualizarTotalGeral();
            }
        };

        aumentarButton.Clicked += (s, e) =>
        {
            _quantidades[produtoAtual.Nome]++;
            quantidadeLabel.Text = _quantidades[produtoAtual.Nome].ToString();
            var valorUnitario = ObterValorUnitario();
            precoLabel.Text = string.Format(CultureInfo.GetCultureInfo("pt-BR"), "R$ {0:N2}", valorUnitario * _quantidades[produtoAtual.Nome]);
            AtualizarTotalGeral();
        };

        botoesPlusMinusStack.Children.Add(aumentarButton);
        botoesPlusMinusStack.Children.Add(diminuirButton);
        quantidadeContainer.Children.Add(botoesPlusMinusStack);

        itemGrid.Children.Add(quantidadeContainer);
        Grid.SetColumn(quantidadeContainer, 3);

        var precoStack = new StackLayout
        {
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.End,
            Spacing = 2
        };

        precoStack.Children.Add(precoLabel);
        itemGrid.Children.Add(precoStack);
        Grid.SetColumn(precoStack, 4);

        itemFrame.Content = itemGrid;

        valorEntry.Unfocused += (s, e) =>
        {
            if (decimal.TryParse(valorEntry.Text, out decimal novoPreco))
            {
                ProdutosPadraoService.AtualizarPrecoPadrao(produtoAtual.Nome, novoPreco);
            }
        };

        valorEntry.Completed += (s, e) =>
        {
            if (decimal.TryParse(valorEntry.Text, out decimal novoPreco))
            {
                ProdutosPadraoService.AtualizarPrecoPadrao(produtoAtual.Nome, novoPreco);
            }
            valorEntry.Unfocus();
        };

        var excluirButton = new Button
        {
            Text = "×",
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            BackgroundColor = Colors.Red,
            TextColor = Colors.White,
            WidthRequest = 30,
            HeightRequest = 30,
            CornerRadius = 15,
            Padding = 0,
            Margin = new Thickness(5, 0, 0, 0),
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.End
        };

        excluirButton.Clicked += async (s, e) => await RemoverProdutoDinamicamente(produtoAtual, itemContainer, containerStack);

        itemContainer.Children.Add(itemFrame);
        Grid.SetColumn(itemFrame, 0);

        itemContainer.Children.Add(excluirButton);
        Grid.SetColumn(excluirButton, 1);

        containerStack.Children.Add(itemContainer);
        _produtoContainers[produto.Nome] = itemContainer;
    }

    private async Task RemoverProdutoDinamicamente((string Nome, UnidadeMedida Unidade, Categoria Categoria, string Icone, decimal PrecoMedio) produto, View itemContainer, StackLayout containerStack)
    {
        // Criar overlay de loading
        var loadingOverlay = new Grid
        {
            BackgroundColor = Color.FromArgb("#80000000"),
            VerticalOptions = LayoutOptions.Fill,
            HorizontalOptions = LayoutOptions.Fill
        };

        var loadingFrame = new Frame
        {
            BackgroundColor = Colors.White,
            CornerRadius = 15,
            Padding = 30,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            HasShadow = true
        };

        var loadingStack = new StackLayout
        {
            Spacing = 10,
            HorizontalOptions = LayoutOptions.Center
        };
         
       
        loadingFrame.Content = loadingStack;
        loadingOverlay.Children.Add(loadingFrame);

        var mainGrid = Content as Grid;
        if (mainGrid != null)
        {
            mainGrid.Children.Add(loadingOverlay);
            Grid.SetRowSpan(loadingOverlay, 2);
        }

        
        // Executar exclusão no serviço
        bool sucesso = ProdutosPadraoService.ExcluirProdutoPadrao(produto.Nome);

        if (sucesso)
        {
            // Remover overlay
            if (mainGrid != null)
            {
                mainGrid.Children.Remove(loadingOverlay);
            }

            // Animação de fade out
            await itemContainer.FadeTo(0, 200);

            // Remover da UI
            containerStack.Children.Remove(itemContainer);

            // Remover dos dicionários
            _produtoContainers.Remove(produto.Nome);
            _checkboxes.Remove(produto.Nome);
            _checkboxStates.Remove(produto.Nome);
            _quantidades.Remove(produto.Nome);
            _valoresUnitarios.Remove(produto.Nome);

            // Verificar se a categoria ficou vazia
            if (containerStack.Children.Count == 0 && _categoriasUI.ContainsKey(produto.Categoria))
            {
                var (categoriaLabel, _) = _categoriasUI[produto.Categoria];

                // Animar e remover o label da categoria
                await categoriaLabel.FadeTo(0, 200);
                _mainStackLayout.Children.Remove(categoriaLabel);
                _mainStackLayout.Children.Remove(containerStack);

                _categoriasUI.Remove(produto.Categoria);
            }

            // Atualizar totais
            AtualizarTotalGeral();
            AtualizarTotalCheckados();
        }
        else
        {
            // Remover overlay em caso de erro
            if (mainGrid != null)
            {
                mainGrid.Children.Remove(loadingOverlay);
            }
        }
    }

    private string ObterTextoCategoria(Categoria categoria)
    {
        return categoria switch
        {
            Categoria.FrutasVerduras => "Frutas e Verduras",
            Categoria.Carnes => "Carnes",
            Categoria.Laticinios => "Laticínios",
            Categoria.Bebidas => "Bebidas",
            Categoria.Limpeza => "Limpeza",
            Categoria.Padaria => "Padaria",
            Categoria.Congelados => "Congelados",
            Categoria.Higiene => "Higiene",
            Categoria.AlimentosBasicos => "Alimentos Básicos",
            Categoria.EnlatadosConservas => "Enlatados e Conservas",
            Categoria.BiscoitosSnacks => "Biscoitos e Snacks",
            Categoria.DocesSobremesas => "Doces e Sobremesas",
            Categoria.TemperoCondimentos => "Temperos e Condimentos",
            Categoria.CafesChas => "Cafés e Chás",
            Categoria.BebeInfantil => "Bebê e Infantil",
            Categoria.PetShop => "Pet Shop",
            Categoria.UtilidadesDomesticas => "Utilidades Domésticas",
            Categoria.Descartaveis => "Descartáveis",
            Categoria.HortifrutiEspeciais => "Hortifruti Especiais",
            Categoria.ComidasProntas => "Comidas Prontas",
            Categoria.Farmacia => "Farmácia",
            Categoria.FestasDecoracao => "Festas e Decoração",
            _ => "Lista Geral(todos os produtos)"
        };
    }

    private void LoadProducts()
    {
    }

    private async void OnEditarListaClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//main");
    }

    private async void OnFinalizarCompraClicked(object sender, EventArgs e)
    {
        var result = await DisplayAlert("Finalizar Compra",
            "Deseja finalizar esta lista de compras?",
            "Sim", "Não");

        if (result)
        {
            await DisplayAlert("Sucesso",
                "Lista finalizada! Uma nova lista padrão será criada.",
                "OK");
        }
    }

    private void AtualizarTotalGeral()
    {
        decimal totalGeral = 0;

        foreach (var kvp in _quantidades)
        {
            var nomeProduto = kvp.Key;
            var quantidade = kvp.Value;

            if (_valoresUnitarios.ContainsKey(nomeProduto))
            {
                var valorEntry = _valoresUnitarios[nomeProduto];
                if (decimal.TryParse(valorEntry.Text, out decimal valorUnitario))
                {
                    totalGeral += valorUnitario * quantidade;
                }
            }
        }
    }

    private void AtualizarTotalCheckados()
    {
        decimal totalCheckados = 0;

        foreach (var kvp in _checkboxes)
        {
            var nomeProduto = kvp.Key;
            var checkbox = kvp.Value;

            if (checkbox.IsChecked)
            {
                var quantidade = _quantidades[nomeProduto];

                if (_valoresUnitarios.ContainsKey(nomeProduto))
                {
                    var valorEntry = _valoresUnitarios[nomeProduto];
                    if (decimal.TryParse(valorEntry.Text, out decimal valorUnitario))
                    {
                        totalCheckados += valorUnitario * quantidade;
                    }
                }
            }
        }

        TotaisManager.UpdateTotalComprado(totalCheckados);
    }

    private async void OnAdicionarCategoriaClicked(object sender, EventArgs e)
    {
        foreach (var kvp in _checkboxes)
        {
            _checkboxStates[kvp.Key] = kvp.Value.IsChecked;
        }

        var categoriaSelecionada = await MostrarPopupCategorias();

        if (categoriaSelecionada.HasValue)
        {
            await Task.Delay(300);
            await MostrarSelecaoProdutos(categoriaSelecionada.Value);
        }
    }

    private async Task<Categoria?> MostrarPopupCategorias()
    {
        var tcs = new TaskCompletionSource<Categoria?>();

        var popupPage = new ContentPage
        {
            BackgroundColor = Color.FromArgb("#80000000")
        };

        var frame = new Frame
        {
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Fill,
            Margin = new Thickness(20),
            CornerRadius = 15,
            Padding = 0,
            HasShadow = true,
            BackgroundColor = Colors.White
        };

        var mainGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto }
            },
            Padding = 20
        };

        var tituloLabel = new Label
        {
            Text = "Selecione uma categoria",
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 0, 15)
        };
        mainGrid.Children.Add(tituloLabel);
        Grid.SetRow(tituloLabel, 0);

        var scrollView = new ScrollView { MaximumHeightRequest = 500 };
        var categoriasStack = new StackLayout { Spacing = 10 };

        var categorias = new List<(Categoria Categoria, string Nome, string Icone)>
        {
            (Categoria.FrutasVerduras, "Frutas e Verduras", "🍎"),
            (Categoria.Carnes, "Carnes", "🥩"),
            (Categoria.Laticinios, "Laticínios", "🥛"),
            (Categoria.Bebidas, "Bebidas", "🥤"),
            (Categoria.Limpeza, "Limpeza", "🧹"),
            (Categoria.Padaria, "Padaria", "🍞"),
            (Categoria.Congelados, "Congelados", "🧊"),
            (Categoria.Higiene, "Higiene", "🧴"),
            (Categoria.AlimentosBasicos, "Alimentos Básicos", "🍚"),
            (Categoria.EnlatadosConservas, "Enlatados e Conservas", "🥫"),
            (Categoria.BiscoitosSnacks, "Biscoitos e Snacks", "🍪"),
            (Categoria.DocesSobremesas, "Doces e Sobremesas", "🍫"),
            (Categoria.TemperoCondimentos, "Temperos e Condimentos", "🧂"),
            (Categoria.CafesChas, "Cafés e Chás", "☕"),
            (Categoria.BebeInfantil, "Bebê e Infantil", "🍼"),
            (Categoria.PetShop, "Pet Shop", "🐕"),
            (Categoria.UtilidadesDomesticas, "Utilidades Domésticas", "🥘"),
            (Categoria.Descartaveis, "Descartáveis", "🥤"),
            (Categoria.HortifrutiEspeciais, "Hortifruti Especiais", "🌿"),
            (Categoria.ComidasProntas, "Comidas Prontas", "🍕"),
            (Categoria.Farmacia, "Farmácia", "💊"),
            (Categoria.FestasDecoracao, "Festas e Decoração", "🎉"),
            (Categoria.ListaGeral, "Lista Geral(todos os produtos)", "📦")
        };

        foreach (var cat in categorias)
        {
            var itemFrame = new Frame
            {
                Padding = 15,
                CornerRadius = 10,
                HasShadow = false,
                BorderColor = Colors.LightGray,
                BackgroundColor = Colors.White
            };

            var categoriaCapturada = cat.Categoria;
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += async (s, e) =>
            {
                tcs.TrySetResult(categoriaCapturada);
                await Navigation.PopModalAsync();
            };
            itemFrame.GestureRecognizers.Add(tapGesture);

            var itemGrid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                }
            };

            var iconeLabel = new Label
            {
                Text = cat.Icone,
                FontSize = 30,
                VerticalOptions = LayoutOptions.Center
            };
            itemGrid.Children.Add(iconeLabel);
            Grid.SetColumn(iconeLabel, 0);

            var nomeLabel = new Label
            {
                Text = cat.Nome,
                FontSize = 18,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(15, 0, 0, 0)
            };
            itemGrid.Children.Add(nomeLabel);
            Grid.SetColumn(nomeLabel, 1);

            itemFrame.Content = itemGrid;
            categoriasStack.Children.Add(itemFrame);
        }

        scrollView.Content = categoriasStack;
        mainGrid.Children.Add(scrollView);
        Grid.SetRow(scrollView, 1);

        var cancelarButton = new Button
        {
            Text = "Cancelar",
            BackgroundColor = Colors.Gray,
            TextColor = Colors.White,
            CornerRadius = 8,
            Margin = new Thickness(0, 15, 0, 0)
        };
        cancelarButton.Clicked += async (s, e) =>
        {
            tcs.TrySetResult(null);
            await Navigation.PopModalAsync();
        };
        mainGrid.Children.Add(cancelarButton);
        Grid.SetRow(cancelarButton, 2);

        frame.Content = mainGrid;
        popupPage.Content = frame;

        await Navigation.PushModalAsync(popupPage);

        return await tcs.Task;
    }

    private string ObterNomeCategoria(Categoria categoria)
    {
        return categoria switch
        {
            Categoria.ListaGeral => "Lista Geral(todos os produtos)",
            Categoria.FrutasVerduras => "Frutas e Verduras",
            Categoria.Carnes => "Carnes",
            Categoria.Laticinios => "Laticínios",
            Categoria.Bebidas => "Bebidas",
            Categoria.Limpeza => "Limpeza",
            Categoria.Padaria => "Padaria",
            Categoria.Congelados => "Congelados",
            Categoria.Higiene => "Higiene",
            _ => "Outros"
        };
    }

    private async Task MostrarSelecaoProdutos(Categoria categoria)
    {
        foreach (var kvp in _checkboxes)
        {
            _checkboxStates[kvp.Key] = kvp.Value.IsChecked;
        }

        var popupPage = new ContentPage
        {
            BackgroundColor = Color.FromArgb("#80000000")
        };

        var frame = new Frame
        {
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Fill,
            Margin = new Thickness(20),
            CornerRadius = 15,
            Padding = 0,
            HasShadow = true,
            BackgroundColor = Colors.White
        };

        var mainGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto }
            },
            Padding = 20
        };

        var tituloLabel = new Label
        {
            Text = $"Selecione um produto para {ObterNomeCategoria(categoria)}",
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 0, 15),
            LineBreakMode = LineBreakMode.WordWrap,
            HorizontalTextAlignment = TextAlignment.Center
        };
        mainGrid.Children.Add(tituloLabel);
        Grid.SetRow(tituloLabel, 0);

        var scrollView = new ScrollView { MaximumHeightRequest = 500 };
        var produtosStack = new StackLayout { Spacing = 10 };

        if (categoria == Categoria.ListaGeral)
        {
            var todosProdutos = ProdutosCatalogo.ObterProdutosPorCategoria();
            List<(string Nome, UnidadeMedida Unidade, string Icone, decimal Preco)> todosProdutosLista = new();

            foreach (var catProdutos in todosProdutos.Values)
            {
                todosProdutosLista.AddRange(catProdutos);
            }

            todosProdutosLista = todosProdutosLista.OrderBy(p => p.Nome).ToList();

            foreach (var produto in todosProdutosLista)
            {
                CriarItemSelecaoProduto(produto.Nome, produto.Unidade, produto.Icone, produto.Preco, categoria, produtosStack);
            }
        }
        else
        {
            var todosProdutos = ProdutosCatalogo.ObterProdutosPorCategoria();
            var produtosDaCategoria = todosProdutos.ContainsKey(categoria)
                ? todosProdutos[categoria]
                : new List<(string, UnidadeMedida, string, decimal)>();

            foreach (var produto in produtosDaCategoria)
            {
                CriarItemSelecaoProduto(produto.Item1, produto.Item2, produto.Item3, produto.Item4, categoria, produtosStack);
            }
        }

        scrollView.Content = produtosStack;
        mainGrid.Children.Add(scrollView);
        Grid.SetRow(scrollView, 1);

        var cancelarButton = new Button
        {
            Text = "Cancelar",
            BackgroundColor = Colors.Gray,
            TextColor = Colors.White,
            CornerRadius = 8,
            Margin = new Thickness(0, 15, 0, 0)
        };
        cancelarButton.Clicked += async (s, e) =>
        {
            await Navigation.PopModalAsync();
        };
        mainGrid.Children.Add(cancelarButton);
        Grid.SetRow(cancelarButton, 2);

        frame.Content = mainGrid;
        popupPage.Content = frame;

        await Navigation.PushModalAsync(popupPage);
    }

    private void CriarItemSelecaoProduto(string nome, UnidadeMedida unidade, string icone, decimal preco, Categoria categoria, StackLayout produtosStack)
    {
        var itemFrame = new Frame
        {
            Padding = 15,
            CornerRadius = 10,
            HasShadow = false,
            BorderColor = Colors.LightGray,
            BackgroundColor = Colors.White
        };

        var nomeProduto = nome;
        var unidadeProduto = unidade;
        var iconeProduto = icone;
        var precoProduto = preco;

        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += async (s, e) =>
        {
            ProdutosPadraoService.AdicionarProdutoPadrao(
                nomeProduto, unidadeProduto, categoria, iconeProduto, precoProduto);

            await Navigation.PopModalAsync();

            // Adicionar produto dinamicamente sem reconstruir
            var novoProduto = (nomeProduto, unidadeProduto, categoria, iconeProduto, precoProduto);

            // Verificar se a categoria já existe
            if (_categoriasUI.ContainsKey(categoria))
            {
                var (_, produtosStackCategoria) = _categoriasUI[categoria];
                AdicionarProdutoNaUI(novoProduto, produtosStackCategoria);
            }
            else
            {
                // Criar nova seção de categoria
                CriarSecaoCategoria(categoria, new List<(string, UnidadeMedida, Categoria, string, decimal)> { novoProduto });
            }

            AtualizarTotalGeral();
        };
        itemFrame.GestureRecognizers.Add(tapGesture);

        var itemGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        var iconeLabel = new Label
        {
            Text = icone,
            FontSize = 30,
            VerticalOptions = LayoutOptions.Center
        };
        itemGrid.Children.Add(iconeLabel);
        Grid.SetColumn(iconeLabel, 0);

        var detalhesStack = new StackLayout
        {
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(15, 0, 0, 0),
            Spacing = 2
        };

        var nomeLabel = new Label
        {
            Text = nome,
            FontSize = 16,
            FontAttributes = FontAttributes.Bold
        };
        detalhesStack.Children.Add(nomeLabel);

        var unidadeTexto = unidade switch
        {
            UnidadeMedida.Kilo => "kg",
            UnidadeMedida.Grama => "g",
            UnidadeMedida.Litro => "L",
            UnidadeMedida.Unidade => "un",
            UnidadeMedida.Pacote => "pct",
            UnidadeMedida.Caixa => "cx",
            _ => "un"
        };

        var unidadeLabel = new Label
        {
            Text = unidadeTexto,
            FontSize = 12,
            TextColor = Colors.Gray
        };
        detalhesStack.Children.Add(unidadeLabel);

        itemGrid.Children.Add(detalhesStack);
        Grid.SetColumn(detalhesStack, 1);

        var precoLabel = new Label
        {
            Text = string.Format(CultureInfo.GetCultureInfo("pt-BR"), "R$ {0:N2}", preco),
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.Green,
            VerticalOptions = LayoutOptions.Center
        };
        itemGrid.Children.Add(precoLabel);
        Grid.SetColumn(precoLabel, 2);

        itemFrame.Content = itemGrid;
        produtosStack.Children.Add(itemFrame);
    }
}