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
    private Label _totalLabel;
    private Label _totalCheckadosLabel;

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
        // Definir cultura brasileira para toda a UI
        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("pt-BR");
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("pt-BR");

        // Grid principal para permitir um rodapé fixo
        var mainGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // Conteúdo principal
                new RowDefinition { Height = GridLength.Auto } // Barra de totais
            }
        };

        var scrollView = new ScrollView();
        var stackLayout = new StackLayout { Padding = 20 };

        // Header da lista
        var headerLabel = new Label
        {
            Text = "Lista de Compras Padrão",
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 0, 0)
        };
        stackLayout.Children.Add(headerLabel);

        // Mostrar produtos como lista de compras
        var produtos = ProdutosPadraoService.ObterProdutosAtivos();
        var categorias = produtos.GroupBy(p => p.Categoria);

        foreach (var categoria in categorias)
        {
            // Título da categoria
            var categoriaTexto = categoria.Key switch
            {
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

            var categoriaLabel = new Label
            {
                Text = categoriaTexto,
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Purple,
                Margin = new Thickness(0, 15, 0, 5)
            };
            stackLayout.Children.Add(categoriaLabel);

            // Itens da categoria
            foreach (var produto in categoria)
            {
                // Container externo que conterá o frame e o botão de exclusão
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
                        new ColumnDefinition { Width = GridLength.Auto }, // Checkbox 
                        new ColumnDefinition { Width = GridLength.Auto }, // Ícone 
                        new ColumnDefinition { Width = new GridLength(4, GridUnitType.Star) }, // Nome + detalhes
                        new ColumnDefinition { Width = new GridLength(90, GridUnitType.Absolute) }, // Controles quantidade 
                        new ColumnDefinition { Width = new GridLength(75, GridUnitType.Absolute) }  // Preço (aumentado para evitar quebras)
                    }
                };

                // Checkbox
                var checkbox = new CheckBox
                {
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    Margin = new Thickness(-10, 0, 0, 0) // Margem negativa à esquerda para puxar mais para a borda
                };
                _checkboxes[produto.Nome] = checkbox;
                checkbox.CheckedChanged += (s, e) => AtualizarTotalCheckados();
                itemGrid.Children.Add(checkbox);
                Grid.SetColumn(checkbox, 0);

                // Ícone
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

                // Inicializar quantidade para este produto
                _quantidades[produto.Nome] = 1;

                // Nome e detalhes
                var detalhesStack = new StackLayout
                {
                    VerticalOptions = LayoutOptions.Start,
                    HorizontalOptions = LayoutOptions.Start,
                    Margin = new Thickness(5, 0, 10, 0),
                    Spacing = 2
                };

                // Verificar se é palavra única ou composta
                var nomeUpper = produto.Nome.ToUpper();
                var temEspacos = nomeUpper.Contains(' ');

                var nomeLabel = new Label
                {
                    Text = nomeUpper,
                    FontSize = 14,
                    FontAttributes = FontAttributes.Bold,
                    LineBreakMode = temEspacos ? LineBreakMode.WordWrap : LineBreakMode.NoWrap,
                    MaxLines = temEspacos ? 2 : 1, // Reduzido para no máximo 2 linhas,
                    VerticalOptions = LayoutOptions.Start,
                    HorizontalOptions = LayoutOptions.Start,
                    WidthRequest = 120
                };

                // Ajustar fonte para palavras longas
                if (nomeUpper.Length > 8) // Reduzido o limite para começar a diminuir a fonte mais cedo
                {
                    nomeLabel.FontSize = 12; // Fonte menor para palavras longas
                }
                if (nomeUpper.Length > 12) // Reduzido o limite para palavras muito longas
                {
                    nomeLabel.FontSize = 10; // Ainda menor para palavras muito longas
                }
                if (nomeUpper.Length > 18) // Caso extremo
                {
                    nomeLabel.FontSize = 9; // Fonte mínima para palavras extremamente longas
                }
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

                // MODIFICAÇÃO: Unidade e quantidade em um layout horizontal
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

                // Adicionar a quantidade após a unidade
                var quantidadeLabel = new Label
                {
                    Text = _quantidades[produto.Nome].ToString(),
                    FontSize = 10,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.Gray,
                    VerticalOptions = LayoutOptions.Center
                };
                unidadeQuantidadeStack.Children.Add(quantidadeLabel);

                // Adicionar o stack de unidade+quantidade ao stack de detalhes
                detalhesStack.Children.Add(unidadeQuantidadeStack);

                // Adicionar stack de detalhes ao grid
                itemGrid.Children.Add(detalhesStack);
                Grid.SetColumn(detalhesStack, 2);

                // Criar label de preço com formatação de moeda brasileira
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

                // Campo de entrada para valor unitário
                var valorEntry = new Entry
                {
                    Placeholder = "0,00",
                    Text = produto.PrecoMedio.ToString("N2", CultureInfo.GetCultureInfo("pt-BR")),
                    FontSize = 12,
                    Keyboard = Keyboard.Numeric,
                    WidthRequest = 65,
                    HeightRequest = 50,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center, // Centralizado verticalmente
                    HorizontalTextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, -8, 0, 0) // Margem negativa no topo para subir o campo
                };

                // Armazenar referência do Entry
                _valoresUnitarios[produto.Nome] = valorEntry;

                // Capturar produto atual para uso nos eventos
                var produtoAtual = produto;
                var isUserInput = false;

                // Selecionar todo o texto quando ganhar foco
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

                // Limpar seleção quando começar a digitar
                valorEntry.TextChanged += (s, e) =>
                {
                    var entry = s as Entry;
                    if (entry != null)
                    {
                        // Evitar processamento durante formatação automática
                        if (isUserInput) return;
                        isUserInput = true;

                        // Remove caracteres não numéricos
                        var numericText = new string(entry.Text.Where(char.IsDigit).ToArray());

                        if (!string.IsNullOrEmpty(numericText))
                        {
                            // Converte para decimal considerando centavos
                            if (long.TryParse(numericText, out long value))
                            {
                                var formattedValue = (decimal)value / 100;

                                var expectedText = formattedValue.ToString("N2", CultureInfo.GetCultureInfo("pt-BR"));

                                entry.Text = expectedText;
                                entry.CursorPosition = expectedText.Length;

                                // Atualiza o preço total do item
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

                // Container horizontal para o Entry e botões verticais
                var quantidadeContainer = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Start,
                    Spacing = 5,
                    Padding = new Thickness(0, 0, 0, 0)
                };

                // Adicionar o Entry à esquerda
                quantidadeContainer.Children.Add(valorEntry);

                // Container vertical para os botões
                var botoesPlusMinusStack = new StackLayout
                {
                    Orientation = StackOrientation.Vertical,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Start,
                    Spacing = 2,
                    Margin = new Thickness(0, 0, 0, 0)
                };

                // Botões de controle
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

                // Função para obter valor unitário atual
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

                // Eventos dos botões
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

                // Adicionar os botões ao stack vertical
                botoesPlusMinusStack.Children.Add(aumentarButton);
                botoesPlusMinusStack.Children.Add(diminuirButton);

                // Adicionar o stack de botões ao container horizontal
                quantidadeContainer.Children.Add(botoesPlusMinusStack);

                // Adicionar o container ao Grid
                itemGrid.Children.Add(quantidadeContainer);
                Grid.SetColumn(quantidadeContainer, 3);

                // Container para preço
                var precoStack = new StackLayout
                {
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.End,
                    Spacing = 2
                };

                // Adicionar o preço
                precoStack.Children.Add(precoLabel);

                // Adicionar o stack de preço ao Grid
                itemGrid.Children.Add(precoStack);
                Grid.SetColumn(precoStack, 4);

                // Configurar o conteúdo do frame
                itemFrame.Content = itemGrid;

                // Botão de exclusão (fora do frame)
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

                // Capturar produto para o evento de exclusão
                var produtoParaExcluir = produto;
                excluirButton.Clicked += async (s, e) => await OnExcluirProdutoClicked(produtoParaExcluir);

                // Adicionar o frame e o botão ao container
                itemContainer.Children.Add(itemFrame);
                Grid.SetColumn(itemFrame, 0);

                itemContainer.Children.Add(excluirButton);
                Grid.SetColumn(excluirButton, 1);

                // Adicionar o container ao stackLayout principal
                stackLayout.Children.Add(itemContainer);
            }
        }

        // Botões de ação
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
        stackLayout.Children.Add(botoesStack);

        scrollView.Content = stackLayout;

        // Adicionar ScrollView à primeira linha do Grid principal
        mainGrid.Children.Add(scrollView);
        Grid.SetRow(scrollView, 0);

        // Criar a barra de totais
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

        // Label para o TOTAL
        _totalLabel = new Label
        {
            Text = string.Format(CultureInfo.GetCultureInfo("pt-BR"), "TOTAL PREVITSO:\nR$ {0:N2}", produtos.Sum(p => p.PrecoMedio)),
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.Green,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        totaisGrid.Children.Add(_totalLabel);
        Grid.SetColumn(_totalLabel, 0);

        // Label para o TOTAL COMPRADO
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

        // Inscrever-se nos eventos do gerenciador de totais
        TotaisManager.TotalChanged += (newText) => _totalLabel.Text = newText;
        TotaisManager.TotalCompradoChanged += (newText) => _totalCheckadosLabel.Text = newText;

        totaisFrame.Content = totaisGrid;

        // Adicionar a barra de totais à segunda linha do Grid principal
        mainGrid.Children.Add(totaisFrame);
        Grid.SetRow(totaisFrame, 1);

        // Definir o Grid principal como o conteúdo da página
        Content = mainGrid;
    }

    private void LoadProducts()
    {
        // Os produtos já são carregados na CreateUI
    }

    // Método para reconstruir a UI
    private void ReconstruirUI()
    {
        Content = null; // Força liberação dos recursos
        CreateUI();
        AtualizarTotalGeral();
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

        // Atualizar o total através do gerenciador
        TotaisManager.UpdateTotal(totalGeral);

        AtualizarTotalCheckados();
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

        // Atualizar o total comprado através do gerenciador
        TotaisManager.UpdateTotalComprado(totalCheckados);
    }

    private async Task OnExcluirProdutoClicked((string Nome, UnidadeMedida Unidade, Categoria Categoria, string Icone, decimal PrecoMedio) produto)
    {
        bool confirmar = await DisplayAlert(
            "Confirmar exclusão",
            $"Tem certeza que deseja excluir {produto.Nome}?",
            "Sim", "Não");

        if (confirmar)
        {
            bool sucesso = ProdutosPadraoService.ExcluirProdutoPadrao(produto.Nome);

            if (sucesso)
            {
                Content = null; // Força liberação dos recursos
                CreateUI();
                AtualizarTotalGeral();
                
            }
           
        }
    }
}