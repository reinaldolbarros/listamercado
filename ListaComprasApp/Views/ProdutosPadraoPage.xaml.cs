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
    private readonly Dictionary<string, Frame> _produtoFrames = new();
    private readonly Dictionary<string, Grid> _produtoGrids = new();

    // Estados separados por mês
    private readonly Dictionary<DateTime, Dictionary<string, bool>> _checkboxStatesPorMes = new();
    private readonly Dictionary<DateTime, Dictionary<string, int>> _quantidadesPorMes = new();

    private Label _totalLabel;
    private Label _totalCheckadosLabel;
    private ScrollView _scrollView;

    private readonly Dictionary<string, View> _produtoContainers = new();
    private readonly Dictionary<Categoria, (Label CategoriaLabel, StackLayout ProdutosStack)> _categoriasUI = new();
    private StackLayout _mainStackLayout;

    private DateTime _mesAtual = DateTime.Now;
    private Label _mesLabel;
    private Button _voltarMesButton;
    private Button _avancarMesButton;

    public ProdutosPadraoPage(ProdutosPadraoViewModel viewModel)
    {
        _viewModel = viewModel;
        BindingContext = viewModel;
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
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto }
            }
        };

        var tituloLabel = new Label
        {
            Text = "Lista de Compras",
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.Black,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Start,
            Margin = new Thickness(15, 10)
        };
        mainGrid.Children.Add(tituloLabel);
        Grid.SetRow(tituloLabel, 0);

        _scrollView = new ScrollView();
        _mainStackLayout = new StackLayout { Padding = new Thickness(15, 0, 15, 20) };

        var controlesSuperioresGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            },
            Margin = new Thickness(0, 0, 0, 15)
        };

        var navegacaoMesStack = new StackLayout
        {
            Orientation = StackOrientation.Horizontal,
            Spacing = 5,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Start
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
            Padding = 0,
            BorderColor = Colors.Purple,
            BorderWidth = 1
        };
        _voltarMesButton.Clicked += OnVoltarMesClicked;

        _mesLabel = new Label
        {
            Text = ObterNomeMes(_mesAtual),
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.Purple,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            MinimumWidthRequest = 100,
            HorizontalTextAlignment = TextAlignment.Center
        };
        // Criar label de indicador de modo
        var _modoLabel = new Label
        {
            Text = "",
            FontSize = 11,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#FF6B6B"),
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            IsVisible = false
        };
        navegacaoMesStack.Children.Add(_modoLabel);

        // Atualizar no método AtualizarBotoesNavegacao():
        if (EhMesPassado(_mesAtual))
        {
            _modoLabel.Text = "📋 MODO VISUALIZAÇÃO";
            _modoLabel.IsVisible = true;
        }
        else
        {
            _modoLabel.IsVisible = false;
        }
        _avancarMesButton = new Button
        {
            Text = "▶",
            FontSize = 18,
            BackgroundColor = Colors.White,
            TextColor = Colors.Purple,
            WidthRequest = 40,
            HeightRequest = 40,
            CornerRadius = 20,
            Padding = 0,
            BorderColor = Colors.Purple,
            BorderWidth = 1
        };
        _avancarMesButton.Clicked += OnAvancarMesClicked;

        navegacaoMesStack.Children.Add(_voltarMesButton);
        navegacaoMesStack.Children.Add(_mesLabel);
        navegacaoMesStack.Children.Add(_avancarMesButton);

        controlesSuperioresGrid.Children.Add(navegacaoMesStack);
        Grid.SetColumn(navegacaoMesStack, 0);

        AtualizarBotoesNavegacao();

        var adicionarCategoriaButton = new Button
        {
            Text = "+ Novo produto",
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            BackgroundColor = Colors.Purple,
            TextColor = Colors.White,
            CornerRadius = 8,
            HeightRequest = 45,
            HorizontalOptions = LayoutOptions.End
        };
        adicionarCategoriaButton.Clicked += OnAdicionarCategoriaClicked;

        controlesSuperioresGrid.Children.Add(adicionarCategoriaButton);
        Grid.SetColumn(adicionarCategoriaButton, 1);

        _mainStackLayout.Children.Add(controlesSuperioresGrid);

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

        var finalizarButton = new Button
        {
            Text = "Finalizar Compra",
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            BackgroundColor = Colors.Green,
            TextColor = Colors.White,
            CornerRadius = 8,
            Padding = new Thickness(20, 10)
        };
        finalizarButton.Clicked += OnFinalizarCompraClicked;

        botoesStack.Children.Add(finalizarButton);
        _mainStackLayout.Children.Add(botoesStack);

        _scrollView.Content = _mainStackLayout;
        mainGrid.Children.Add(_scrollView);
        Grid.SetRow(_scrollView, 1);

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
        Grid.SetRow(totaisFrame, 2);

        Content = mainGrid;

        AtualizarTotalGeral();
        AtualizarTotalCheckados();
    }

    private string ObterNomeMes(DateTime data)
    {
        var cultura = new CultureInfo("pt-BR");
        return cultura.TextInfo.ToTitleCase(data.ToString("MMMM/yyyy", cultura));
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

        // Recarregar a lista com os dados do mês selecionado
        CarregarDadosDoMes();
    }

    private void OnAvancarMesClicked(object sender, EventArgs e)
    {
        _mesAtual = _mesAtual.AddMonths(1);
        _mesLabel.Text = ObterNomeMes(_mesAtual);
        AtualizarBotoesNavegacao();

        // Recarregar a lista com os dados do mês selecionado
        CarregarDadosDoMes();
    }
    /// <summary>
    /// Verifica se o mês atual é um mês passado (já finalizado)
    /// </summary>
    private bool EhMesPassado(DateTime mes)
    {
        var mesAtualReal = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var mesSelecionado = new DateTime(mes.Year, mes.Month, 1);

        return mesSelecionado < mesAtualReal;
    }

    // ========== MÉTODO 3: ADICIONAR (novo método) ==========

    // Substitua o método ReconstruirListaProdutos:

    private void DesabilitarBotoesQuantidade()
    {
        foreach (var container in _produtoContainers.Values)
        {
            // Percorrer hierarquia para encontrar os botões
            if (container is Grid grid)
            {
                foreach (var child in grid.Children)
                {
                    if (child is Frame frame && frame.Content is Grid itemGrid)
                    {
                        foreach (var itemChild in itemGrid.Children)
                        {
                            if (itemChild is StackLayout stackLayout)
                            {
                                DesabilitarBotoesRecursivo(stackLayout);
                            }
                        }
                    }
                }
            }
        }
    }
    /// <summary>
    /// Desabilita botões recursivamente em uma StackLayout
    /// </summary>
    private void DesabilitarBotoesRecursivo(StackLayout stack)
    {
        foreach (var child in stack.Children)
        {
            if (child is Button button)
            {
                button.IsEnabled = false;
                button.Opacity = 0.5;
            }
            else if (child is StackLayout innerStack)
            {
                DesabilitarBotoesRecursivo(innerStack);
            }
        }
    }

    // ========== MÉTODO 4: ADICIONAR (novo método auxiliar) ==========

    /// <summary>
    /// Limpa todas as categorias da interface
    /// </summary>
    private void LimparTodasCategorias()
    {
        if (_mainStackLayout == null) return;

        // REMOVER TUDO exceto os controles superiores e botão finalizar
        var itensParaRemover = new List<IView>();

        // Percorrer todos os children e marcar para remoção
        // (exceto o primeiro item que são os controles superiores)
        for (int i = 1; i < _mainStackLayout.Children.Count; i++)
        {
            var child = _mainStackLayout.Children[i];

            // Não remover o botão "Finalizar Compra" (último item)
            bool ehBotaoFinalizar = child is StackLayout stack &&
                                    stack.Children.Any(c => c is Button btn &&
                                    btn.Text != null && btn.Text.Contains("Finalizar"));

            if (!ehBotaoFinalizar)
            {
                itensParaRemover.Add(child);
            }
        }

        // Remover todos os itens marcados
        foreach (var item in itensParaRemover)
        {
            _mainStackLayout.Children.Remove(item);
        }

        // Limpar dicionários
        _categoriasUI?.Clear();
        _produtoContainers?.Clear();
        _produtoFrames?.Clear();
        _produtoGrids?.Clear();
        _checkboxes?.Clear();
        _quantidades?.Clear();
        _valoresUnitarios?.Clear();
    }
    /// <summary>
    /// Exibe mensagem quando não há compras no mês
    /// </summary>
    private void ExibirMensagemMesVazio()
    {
        var mensagemFrame = new Frame
        {
            BackgroundColor = Color.FromArgb("#FFF8E1"),
            BorderColor = Color.FromArgb("#FFD54F"),
            CornerRadius = 12,
            Padding = new Thickness(30, 20),
            Margin = new Thickness(15, 50, 15, 0),
            HasShadow = false
        };

        var stack = new StackLayout
        {
            Spacing = 10,
            HorizontalOptions = LayoutOptions.Center
        };

        var iconeLabel = new Label
        {
            Text = "📋",
            FontSize = 48,
            HorizontalOptions = LayoutOptions.Center
        };
        stack.Children.Add(iconeLabel);

        var tituloLabel = new Label
        {
            Text = "Nenhuma compra realizada",
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            HorizontalTextAlignment = TextAlignment.Center,
            TextColor = Color.FromArgb("#F57C00")
        };
        stack.Children.Add(tituloLabel);

        var cultura = new CultureInfo("pt-BR");
        var nomeMes = cultura.TextInfo.ToTitleCase(_mesAtual.ToString("MMMM/yyyy", cultura));

        var mensagemLabel = new Label
        {
            Text = $"Não há registros de compras em {nomeMes}",
            FontSize = 14,
            HorizontalTextAlignment = TextAlignment.Center,
            TextColor = Colors.Gray
        };
        stack.Children.Add(mensagemLabel);

        mensagemFrame.Content = stack;
        _mainStackLayout.Children.Add(mensagemFrame);
    }
    // Substitua o método CarregarDadosDoMes COMPLETO:
    // 1. Substitua CarregarDadosDoMes:

    private void CarregarDadosDoMes()
    {
        var mesPrimeiroDia = new DateTime(_mesAtual.Year, _mesAtual.Month, 1);

        if (!_checkboxStatesPorMes.ContainsKey(mesPrimeiroDia))
        {
            _checkboxStatesPorMes[mesPrimeiroDia] = new Dictionary<string, bool>();
        }
        if (!_quantidadesPorMes.ContainsKey(mesPrimeiroDia))
        {
            _quantidadesPorMes[mesPrimeiroDia] = new Dictionary<string, int>();
        }

        bool ehMesPassado = EhMesPassado(_mesAtual);

        if (ehMesPassado)
        {
            ReconstruirListaProdutos(mesPrimeiroDia);
        }
        else
        {
            // MÊS ATUAL: Recarregar tudo
            _ = Task.Run(async () => await RecarregarProdutos());
        }

        AtualizarTotalGeral();
        AtualizarTotalCheckados();
    }

    // 2. Adicione ReconstruirListaProdutos:

    private void ReconstruirListaProdutos(DateTime mesPrimeiroDia)
    {
        try
        {
            if (_mainStackLayout == null) return;

            var historicoMes = HistoricoMensal.ObterHistoricoMes(_mesAtual);

            LimparTodasCategorias();

            // Se não há histórico, mostrar mensagem e SAIR
            if (historicoMes == null || historicoMes.Count == 0)
            {
                ExibirMensagemMesVazio();
                return; // ← SAIR AQUI, não continuar
            }

            var todosProdutos = ProdutosPadraoService.ObterProdutosAtivos();
            if (todosProdutos == null) return;

            var produtosComprados = todosProdutos.Where(p => historicoMes.ContainsKey(p.Nome)).ToList();

            // Se não há produtos comprados, mostrar mensagem e SAIR
            if (produtosComprados.Count == 0)
            {
                ExibirMensagemMesVazio();
                return; // ← SAIR AQUI, não continuar
            }

            // APENAS SE CHEGOU AQUI (tem produtos): criar as categorias
            var categorias = produtosComprados.GroupBy(p => p.Categoria);

            foreach (var categoria in categorias)
            {
                CriarSecaoCategoria(categoria.Key, categoria.ToList());
            }

            // Resto do código...
            foreach (var kvp in _checkboxes.ToList())
            {
                if (kvp.Value != null)
                {
                    kvp.Value.IsChecked = false;
                    kvp.Value.IsEnabled = false;
                }
            }

            foreach (var produto in produtosComprados)
            {
                if (historicoMes.ContainsKey(produto.Nome))
                {
                    var (quantidade, valor) = historicoMes[produto.Nome];

                    if (_quantidades != null)
                        _quantidades[produto.Nome] = quantidade;

                    if (_valoresUnitarios != null && _valoresUnitarios.ContainsKey(produto.Nome))
                    {
                        var entry = _valoresUnitarios[produto.Nome];
                        if (entry != null)
                        {
                            entry.Text = valor.ToString("N2", CultureInfo.GetCultureInfo("pt-BR"));
                            entry.IsEnabled = false;
                        }
                    }
                }
            }

            DesabilitarBotoesQuantidade();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro em ReconstruirListaProdutos: {ex.Message}");
        }
    }
    private void ReordenarProdutosPorEstado()
    {
        foreach (var categoriaUI in _categoriasUI)
        {
            var categoria = categoriaUI.Key;
            var (_, produtosStack) = categoriaUI.Value;

            // Criar lista de produtos desta categoria
            var produtosDaCategoria = new List<(View Container, bool IsChecked, string Nome)>();

            foreach (var containerKvp in _produtoContainers)
            {
                var nomeProduto = containerKvp.Key;
                var container = containerKvp.Value;

                // Verificar se este produto pertence a esta categoria
                var produtoInfo = ProdutosPadraoService.ObterProdutosAtivos()
                    .FirstOrDefault(p => p.Nome == nomeProduto);

                if (produtoInfo.Nome != null && produtoInfo.Categoria == categoria)
                {
                    bool isChecked = _checkboxes.ContainsKey(nomeProduto) && _checkboxes[nomeProduto].IsChecked;
                    produtosDaCategoria.Add((container, isChecked, nomeProduto));
                }
            }

            // Ordenar: não marcados primeiro, depois marcados
            var produtosOrdenados = produtosDaCategoria
                .OrderBy(p => p.IsChecked ? 1 : 0)
                .ThenBy(p => p.Nome)
                .ToList();

            // Limpar e recriar a ordem
            produtosStack.Children.Clear();
            foreach (var produto in produtosOrdenados)
            {
                produtosStack.Children.Add(produto.Container);
            }
        }
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

        var produtosStack = new StackLayout { Spacing = 2 };

        // Encontrar posição correta (após controles superiores e antes do botão finalizar)
        int posicaoInserir = 1; // Após os controles superiores

        // Contar quantas categorias já existem
        foreach (var cat in _categoriasUI.Values)
        {
            posicaoInserir += 2; // Label + Stack de cada categoria
        }

        // Inserir na posição correta em vez de Add
        if (posicaoInserir < _mainStackLayout.Children.Count)
        {
            _mainStackLayout.Children.Insert(posicaoInserir, categoriaLabel);
            _mainStackLayout.Children.Insert(posicaoInserir + 1, produtosStack);
        }
        else
        {
            _mainStackLayout.Children.Add(categoriaLabel);
            _mainStackLayout.Children.Add(produtosStack);
        }

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
            HeightRequest = 62,
            HasShadow = false
        };

        _produtoFrames[produto.Nome] = itemFrame;

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

        _produtoGrids[produto.Nome] = itemGrid;

        var checkbox = new CheckBox
        {
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(-10, 0, 0, 0)
        };

        // Carregar estado do checkbox do mês atual
        var mesPrimeiroDia = new DateTime(_mesAtual.Year, _mesAtual.Month, 1);
        if (!_checkboxStatesPorMes.ContainsKey(mesPrimeiroDia))
        {
            _checkboxStatesPorMes[mesPrimeiroDia] = new Dictionary<string, bool>();
        }

        if (_checkboxStatesPorMes[mesPrimeiroDia].ContainsKey(produto.Nome))
        {
            checkbox.IsChecked = _checkboxStatesPorMes[mesPrimeiroDia][produto.Nome];
        }
        else
        {
            checkbox.IsChecked = false;
            _checkboxStatesPorMes[mesPrimeiroDia][produto.Nome] = false;
        }

        _checkboxes[produto.Nome] = checkbox;

        checkbox.CheckedChanged += CheckboxChangedHandler;

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
            // Carregar quantidade do mês atual
            mesPrimeiroDia = new DateTime(_mesAtual.Year, _mesAtual.Month, 1);
            if (!_quantidadesPorMes.ContainsKey(mesPrimeiroDia))
            {
                _quantidadesPorMes[mesPrimeiroDia] = new Dictionary<string, int>();
            }

            if (_quantidadesPorMes[mesPrimeiroDia].ContainsKey(produto.Nome))
            {
                _quantidades[produto.Nome] = _quantidadesPorMes[mesPrimeiroDia][produto.Nome];
            }
            else
            {
                _quantidades[produto.Nome] = 1;
                _quantidadesPorMes[mesPrimeiroDia][produto.Nome] = 1;
            }
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

                // Salvar quantidade no mês atual
                var mesPrimeiroDia = new DateTime(_mesAtual.Year, _mesAtual.Month, 1);
                _quantidadesPorMes[mesPrimeiroDia][produtoAtual.Nome] = _quantidades[produtoAtual.Nome];

                AtualizarTotalGeral();
            }
        };

        aumentarButton.Clicked += (s, e) =>
        {
            _quantidades[produtoAtual.Nome]++;
            quantidadeLabel.Text = _quantidades[produtoAtual.Nome].ToString();
            var valorUnitario = ObterValorUnitario();
            precoLabel.Text = string.Format(CultureInfo.GetCultureInfo("pt-BR"), "R$ {0:N2}", valorUnitario * _quantidades[produtoAtual.Nome]);

            // Salvar quantidade no mês atual
            var mesPrimeiroDia = new DateTime(_mesAtual.Year, _mesAtual.Month, 1);
            _quantidadesPorMes[mesPrimeiroDia][produtoAtual.Nome] = _quantidades[produtoAtual.Nome];

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
                AtualizarTotalGeral();
                AtualizarTotalCheckados();
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

        if (checkbox.IsChecked)
        {
            AplicarEstiloComprado(produto.Nome);
        }
    }

    private void AplicarEstiloComprado(string nomeProduto)
    {
        if (_produtoFrames.ContainsKey(nomeProduto) && _produtoGrids.ContainsKey(nomeProduto))
        {
            var frame = _produtoFrames[nomeProduto];
            var itemGrid = _produtoGrids[nomeProduto];

            frame.BackgroundColor = Color.FromArgb("#F5F5F5");
            frame.Opacity = 0.6;

            foreach (var child in itemGrid.Children)
            {
                if (child is Label label)
                {
                    label.TextDecorations = TextDecorations.Strikethrough;
                    label.TextColor = Colors.Gray;
                }
                else if (child is StackLayout stack)
                {
                    AplicarRiscadoEmLabels(stack);
                }
            }
        }
    }

    private void AplicarRiscadoEmLabels(StackLayout stack)
    {
        foreach (var child in stack.Children)
        {
            if (child is Label label)
            {
                label.TextDecorations = TextDecorations.Strikethrough;
                label.TextColor = Colors.Gray;
            }
            else if (child is StackLayout innerStack)
            {
                AplicarRiscadoEmLabels(innerStack);
            }
        }
    }

    private void RemoverEstiloComprado(string nomeProduto)
    {
        if (_produtoFrames.ContainsKey(nomeProduto) && _produtoGrids.ContainsKey(nomeProduto))
        {
            var frame = _produtoFrames[nomeProduto];
            var itemGrid = _produtoGrids[nomeProduto];

            frame.BackgroundColor = Colors.White;
            frame.Opacity = 1.0;

            foreach (var child in itemGrid.Children)
            {
                if (child is Label label && label.Text != null)
                {
                    label.TextDecorations = TextDecorations.None;
                    if (label.Text.Contains("R$"))
                    {
                        label.TextColor = Colors.Green;
                    }
                    else if (label.FontAttributes == FontAttributes.Bold && label.FontSize >= 10)
                    {
                        label.TextColor = Colors.Black;
                    }
                    else
                    {
                        label.TextColor = Colors.Gray;
                    }
                }
                else if (child is StackLayout stack)
                {
                    RemoverRiscadoEmLabels(stack);
                }
            }
        }
    }

    private void RemoverRiscadoEmLabels(StackLayout stack)
    {
        foreach (var child in stack.Children)
        {
            if (child is Label label && label.Text != null)
            {
                label.TextDecorations = TextDecorations.None;
                if (label.Text.Contains("R$"))
                {
                    label.TextColor = Colors.Green;
                }
                else if (label.FontAttributes == FontAttributes.Bold && label.FontSize >= 10)
                {
                    label.TextColor = Colors.Black;
                }
                else
                {
                    label.TextColor = Colors.Gray;
                }
            }
            else if (child is StackLayout innerStack)
            {
                RemoverRiscadoEmLabels(innerStack);
            }
        }
    }

    private async Task MoverProdutoParaFinalDaCategoria(string nomeProduto, Categoria categoria)
    {
        if (_produtoContainers.ContainsKey(nomeProduto) && _categoriasUI.ContainsKey(categoria))
        {
            var produtoContainer = _produtoContainers[nomeProduto];
            var (_, produtosStack) = _categoriasUI[categoria];

            produtosStack.Children.Remove(produtoContainer);
            produtosStack.Children.Add(produtoContainer);

            await produtoContainer.FadeTo(0.5, 100);
            await produtoContainer.FadeTo(1, 100);
        }
    }

    private async Task MoverProdutoParaInicioDaCategoria(string nomeProduto, Categoria categoria)
    {
        if (_produtoContainers.ContainsKey(nomeProduto) && _categoriasUI.ContainsKey(categoria))
        {
            var produtoContainer = _produtoContainers[nomeProduto];
            var (_, produtosStack) = _categoriasUI[categoria];

            int posicaoInserir = 0;
            foreach (var child in produtosStack.Children)
            {
                bool encontrouMarcado = false;
                foreach (var kvp in _produtoContainers)
                {
                    if (kvp.Value == child && _checkboxes.ContainsKey(kvp.Key))
                    {
                        if (_checkboxes[kvp.Key].IsChecked)
                        {
                            encontrouMarcado = true;
                            break;
                        }
                    }
                }

                if (encontrouMarcado)
                {
                    break;
                }
                posicaoInserir++;
            }

            produtosStack.Children.Remove(produtoContainer);
            produtosStack.Children.Insert(posicaoInserir, produtoContainer);

            await produtoContainer.FadeTo(0.5, 100);
            await produtoContainer.FadeTo(1, 100);
        }
    }

    private async Task RemoverProdutoDinamicamente((string Nome, UnidadeMedida Unidade, Categoria Categoria, string Icone, decimal PrecoMedio) produto, View itemContainer, StackLayout containerStack)
    {
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
            Grid.SetRowSpan(loadingOverlay, 3);
        }

        bool sucesso = ProdutosPadraoService.ExcluirProdutoPadrao(produto.Nome);

        if (sucesso)
        {
            if (mainGrid != null)
            {
                mainGrid.Children.Remove(loadingOverlay);
            }

            await itemContainer.FadeTo(0, 200);

            containerStack.Children.Remove(itemContainer);

            _produtoContainers.Remove(produto.Nome);
            _produtoFrames.Remove(produto.Nome);
            _produtoGrids.Remove(produto.Nome);
            _checkboxes.Remove(produto.Nome);
            _quantidades.Remove(produto.Nome);
            _valoresUnitarios.Remove(produto.Nome);

            if (containerStack.Children.Count == 0 && _categoriasUI.ContainsKey(produto.Categoria))
            {
                var (categoriaLabel, _) = _categoriasUI[produto.Categoria];

                await categoriaLabel.FadeTo(0, 200);
                _mainStackLayout.Children.Remove(categoriaLabel);
                _mainStackLayout.Children.Remove(containerStack);

                _categoriasUI.Remove(produto.Categoria);
            }

            AtualizarTotalGeral();
            AtualizarTotalCheckados();
        }
        else
        {
            if (mainGrid != null)
            {
                mainGrid.Children.Remove(loadingOverlay);
            }

            await DisplayAlert("Erro", "Não foi possível excluir o produto.", "OK");
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
        ProdutosPersonalizadosService.CarregarDados();

        // Carregar dados do mês atual ao iniciar
        CarregarDadosDoMes();

        AtualizarTotalGeral();
        AtualizarTotalCheckados();
    }

    private async Task RecarregarProdutos()
    {
        await Device.InvokeOnMainThreadAsync(() =>
        {
            try
            {
                if (_mainStackLayout == null) return;

                LimparTodasCategorias();

                var produtos = ProdutosPadraoService.ObterProdutosAtivos();

                if (produtos == null || !produtos.Any()) return;

                var categorias = produtos.GroupBy(p => p.Categoria);

                foreach (var categoria in categorias)
                {
                    if (categoria == null || !categoria.Any()) continue;
                    CriarSecaoCategoria(categoria.Key, categoria.ToList());
                }

                AtualizarTotalGeral();
                AtualizarTotalCheckados();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro em RecarregarProdutos: {ex.Message}");
            }
        });
    }
    private void AtualizarTotalGeral()
    {
        Dictionary<string, decimal> valoresUnitarios = new Dictionary<string, decimal>();

        foreach (var kvp in _valoresUnitarios)
        {
            var nomeProduto = kvp.Key;
            var entry = kvp.Value;

            if (decimal.TryParse(entry.Text, out decimal valorUnitario))
            {
                valoresUnitarios[nomeProduto] = valorUnitario;
            }
            else
            {
                valoresUnitarios[nomeProduto] = 0m;
            }
        }

        decimal totalGeral = TotaisManager.CalcularTotal(_quantidades, valoresUnitarios);
        TotaisManager.UpdateTotal(totalGeral);
    }

    private void AtualizarTotalCheckados()
    {
        Dictionary<string, decimal> valoresUnitarios = new Dictionary<string, decimal>();
        Dictionary<string, bool> itemsChecados = new Dictionary<string, bool>();

        foreach (var kvp in _valoresUnitarios)
        {
            var nomeProduto = kvp.Key;
            var entry = kvp.Value;

            if (decimal.TryParse(entry.Text, out decimal valorUnitario))
            {
                valoresUnitarios[nomeProduto] = valorUnitario;
            }
            else
            {
                valoresUnitarios[nomeProduto] = 0m;
            }
        }

        foreach (var kvp in _checkboxes)
        {
            itemsChecados[kvp.Key] = kvp.Value.IsChecked;
        }

        decimal totalCheckados = TotaisManager.CalcularTotalSelecionado(_quantidades, valoresUnitarios, itemsChecados);
        TotaisManager.UpdateTotalComprado(totalCheckados);
    }

    private async void OnFinalizarCompraClicked(object sender, EventArgs e)
    {
        // Verificar se há produtos marcados
        bool temProdutosMarcados = _checkboxes.Any(kvp => kvp.Value.IsChecked);

        if (!temProdutosMarcados)
        {
            await DisplayAlert("Aviso",
                "Nenhum produto foi marcado. Selecione os produtos usando os checkboxes.",
                "OK");
            return;
        }

        var result = await DisplayAlert("Finalizar Compra",
            $"Deseja finalizar esta lista de compras para {ObterNomeMes(_mesAtual)}? Os produtos marcados serão acumulados no histórico.",
            "Sim", "Não");

        if (result)
        {
            int produtosFinalizados = 0;
            var produtosMarcados = new List<string>();

            foreach (var kvp in _checkboxes.ToList())
            {
                if (kvp.Value.IsChecked)
                {
                    produtosMarcados.Add(kvp.Key);
                }
            }

            foreach (var nomeProduto in produtosMarcados)
            {
                produtosFinalizados++;

                int quantidade = _quantidades.ContainsKey(nomeProduto) ? _quantidades[nomeProduto] : 1;
                decimal valorUnitario = 0;

                if (_valoresUnitarios.ContainsKey(nomeProduto))
                {
                    Entry entry = _valoresUnitarios[nomeProduto];
                    decimal.TryParse(entry.Text, out valorUnitario);
                }

                // IMPORTANTE: Salvar no histórico do MÊS SELECIONADO
                HistoricoMensal.AdicionarItem(_mesAtual, nomeProduto, quantidade, valorUnitario);

                // DEBUG - ver se salvou
                System.Diagnostics.Debug.WriteLine($"Salvou: {nomeProduto} em {_mesAtual:yyyy-MM}");
            }

            await DisplayAlert("Sucesso",
                $"Compra finalizada! {produtosFinalizados} produto(s) foram salvos no histórico de {ObterNomeMes(_mesAtual)}.",
                "OK");

            // RECARREGAR para mostrar os produtos salvos
            CarregarDadosDoMes();
        }
    }
    // Método auxiliar para atualizar o label de quantidade
    private void AtualizarQuantidadeLabel(StackLayout stack, int novaQuantidade)
    {
        foreach (var child in stack.Children)
        {
            if (child is Label label && label.FontAttributes == FontAttributes.Bold && label.FontSize == 10)
            {
                label.Text = novaQuantidade.ToString();
                return;
            }
            else if (child is StackLayout innerStack)
            {
                AtualizarQuantidadeLabel(innerStack, novaQuantidade);
            }
        }
    }

    // Handler separado para o evento CheckedChanged
    private async void CheckboxChangedHandler(object sender, CheckedChangedEventArgs e)
    {
        var checkbox = sender as CheckBox;
        if (checkbox == null) return;

        // Encontrar o nome do produto
        string nomeProduto = null;
        foreach (var kvp in _checkboxes)
        {
            if (kvp.Value == checkbox)
            {
                nomeProduto = kvp.Key;
                break;
            }
        }

        if (nomeProduto == null) return;

        // Salvar estado do checkbox no mês atual
        var mesPrimeiroDia = new DateTime(_mesAtual.Year, _mesAtual.Month, 1);
        if (!_checkboxStatesPorMes.ContainsKey(mesPrimeiroDia))
        {
            _checkboxStatesPorMes[mesPrimeiroDia] = new Dictionary<string, bool>();
        }
        _checkboxStatesPorMes[mesPrimeiroDia][nomeProduto] = checkbox.IsChecked;

        // Encontrar a categoria do produto
        var produtoInfo = ProdutosPadraoService.ObterProdutosAtivos()
            .FirstOrDefault(p => p.Nome == nomeProduto);

        if (produtoInfo.Nome == null) return;

        if (checkbox.IsChecked)
        {
            AplicarEstiloComprado(nomeProduto);
            await MoverProdutoParaFinalDaCategoria(nomeProduto, produtoInfo.Categoria);
        }
        else
        {
            RemoverEstiloComprado(nomeProduto);
            await MoverProdutoParaInicioDaCategoria(nomeProduto, produtoInfo.Categoria);
        }

        AtualizarTotalCheckados();
    }

    private async void OnAdicionarCategoriaClicked(object sender, EventArgs e)
    {
        // Salvar estados do mês atual antes de abrir popup
        var mesPrimeiroDia = new DateTime(_mesAtual.Year, _mesAtual.Month, 1);
        if (!_checkboxStatesPorMes.ContainsKey(mesPrimeiroDia))
        {
            _checkboxStatesPorMes[mesPrimeiroDia] = new Dictionary<string, bool>();
        }

        foreach (var kvp in _checkboxes)
        {
            _checkboxStatesPorMes[mesPrimeiroDia][kvp.Key] = kvp.Value.IsChecked;
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
        // Salvar estados do mês atual antes de abrir popup
        var mesPrimeiroDia = new DateTime(_mesAtual.Year, _mesAtual.Month, 1);
        if (!_checkboxStatesPorMes.ContainsKey(mesPrimeiroDia))
        {
            _checkboxStatesPorMes[mesPrimeiroDia] = new Dictionary<string, bool>();
        }

        foreach (var kvp in _checkboxes)
        {
            _checkboxStatesPorMes[mesPrimeiroDia][kvp.Key] = kvp.Value.IsChecked;
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
            bool sucesso = ProdutosPadraoService.AdicionarProdutoPadrao(
                nomeProduto, unidadeProduto, categoria, iconeProduto, precoProduto);

            if (sucesso)
            {
                await Navigation.PopModalAsync();

                var novoProduto = (nomeProduto, unidadeProduto, categoria, iconeProduto, precoProduto);

                if (_categoriasUI.ContainsKey(categoria))
                {
                    var (_, produtosStackCategoria) = _categoriasUI[categoria];
                    AdicionarProdutoNaUI(novoProduto, produtosStackCategoria);
                }
                else
                {
                    CriarSecaoCategoria(categoria, new List<(string, UnidadeMedida, Categoria, string, decimal)> { novoProduto });
                }

                AtualizarTotalGeral();
                await DisplayAlert("Sucesso", $"{nomeProduto} foi adicionado à lista!", "OK");
            }
            else
            {
                await Navigation.PopModalAsync();
                await DisplayAlert("Aviso", $"{nomeProduto} já está na lista ou não pôde ser adicionado.", "OK");
            }
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