using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ListaComprasApp.Models;
using ListaComprasApp.Services;
using System.Collections.ObjectModel;

namespace ListaComprasApp.ViewModels
{
    [QueryProperty(nameof(ListaId), "listaId")]
    public partial class ProdutosPadraoViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;

        public ObservableCollection<ProdutoPadrao> Produtos { get; } = new();

        [ObservableProperty]
        int listaId;

        [ObservableProperty]
        string filtro = string.Empty;

        [ObservableProperty]
        Categoria? categoriaFiltro;

        public List<Categoria> Categorias { get; } = Enum.GetValues<Categoria>().ToList();

        public ProdutosPadraoViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            Title = "Lista de Compras";

            // Criar lista padrão automaticamente
            _ = Task.Run(async () => await CriarListaPadrao());
        }

        [RelayCommand]
        async Task CriarListaPadrao()
        {
            try
            {
                IsBusy = true;

                // Verificar se já existe uma lista padrão
                var listas = await _databaseService.GetListasAsync();
                var listaPadrao = listas.FirstOrDefault(l => l.Nome == "Lista Padrão");

                if (listaPadrao == null)
                {
                    // Criar nova lista padrão
                    listaPadrao = new ListaCompras
                    {
                        Nome = "Lista Padrão",
                        DataCriacao = DateTime.Now
                    };

                    await _databaseService.SaveListaAsync(listaPadrao);

                    // Recarregar para obter o ID
                    listas = await _databaseService.GetListasAsync();
                    listaPadrao = listas.First(l => l.Nome == "Lista Padrão");
                }

                ListaId = listaPadrao.Id;

                // Adicionar todos os produtos padrão se a lista estiver vazia
                if (listaPadrao.TotalItens == 0)
                {
                    var todosProdutos = ProdutosPadraoService.ObterTodosProdutos();

                    foreach (var produto in todosProdutos)
                    {
                        var item = new Item
                        {
                            Nome = produto.Nome,
                            Icone = produto.Icone,
                            Unidade = produto.Unidade,
                            Categoria = produto.Categoria,
                            ValorUnitario = produto.PrecoMedio,
                            Quantidade = 1,
                            ListaComprasId = listaPadrao.Id
                        };

                        await _databaseService.SaveItemAsync(item);
                    }
                }

                await CarregarProdutos();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao criar lista padrão: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        partial void OnListaIdChanged(int value)
        {
            CarregarProdutosCommand.Execute(null);
        }

        partial void OnFiltroChanged(string value)
        {
            FiltrarProdutos();
        }

        partial void OnCategoriaFiltroChanged(Categoria? value)
        {
            FiltrarProdutos();
        }

        [RelayCommand]
        async Task CarregarProdutos()
        {
            try
            {
                IsBusy = true;
                var produtos = ProdutosPadraoService.ObterTodosProdutos();

                Produtos.Clear();
                foreach (var produto in produtos)
                {
                    Produtos.Add(new ProdutoPadrao
                    {
                        Nome = produto.Nome,
                        Icone = produto.Icone,
                        Unidade = produto.Unidade,
                        Categoria = produto.Categoria,
                        PrecoMedio = produto.PrecoMedio
                    });
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao carregar produtos: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        void FiltrarProdutos()
        {
            var todosProdutos = ProdutosPadraoService.ObterTodosProdutos();

            var produtosFiltrados = todosProdutos.Where(p =>
            {
                bool passaFiltroNome = string.IsNullOrWhiteSpace(Filtro) ||
                                      p.Nome.Contains(Filtro, StringComparison.OrdinalIgnoreCase);

                bool passaFiltroCategoria = CategoriaFiltro == null || p.Categoria == CategoriaFiltro;

                return passaFiltroNome && passaFiltroCategoria;
            });

            Produtos.Clear();
            foreach (var produto in produtosFiltrados)
            {
                Produtos.Add(new ProdutoPadrao
                {
                    Nome = produto.Nome,
                    Icone = produto.Icone,
                    Unidade = produto.Unidade,
                    Categoria = produto.Categoria,
                    PrecoMedio = produto.PrecoMedio
                });
            }
        }

        [RelayCommand]
        async Task AdicionarProduto(ProdutoPadrao produto)
        {
            if (produto == null) return;

            try
            {
                // Se não tiver ListaId, criar uma lista automaticamente
                if (ListaId == 0)
                {
                    var novaLista = new ListaCompras
                    {
                        Nome = "Lista de Compras",
                        DataCriacao = DateTime.Now
                    };

                    await _databaseService.SaveListaAsync(novaLista);

                    // Obter o ID da lista recém-criada
                    var listas = await _databaseService.GetListasAsync();
                    var ultimaLista = listas.OrderByDescending(l => l.DataCriacao).First();
                    ListaId = ultimaLista.Id;
                }

                var novoItem = new Item
                {
                    Nome = produto.Nome,
                    Icone = produto.Icone,
                    Unidade = produto.Unidade,
                    Categoria = produto.Categoria,
                    ValorUnitario = produto.PrecoMedio,
                    Quantidade = 1,
                    ListaComprasId = ListaId
                };

                await _databaseService.SaveItemAsync(novoItem);

                await Shell.Current.DisplayAlert("Sucesso", $"{produto.Nome} adicionado à lista!", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao adicionar produto: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        async Task LimparFiltros()
        {
            Filtro = string.Empty;
            CategoriaFiltro = null;
            await CarregarProdutos();
        }
    }

    public class ProdutoPadrao
    {
        public string Nome { get; set; } = string.Empty;
        public string Icone { get; set; } = "📦";
        public UnidadeMedida Unidade { get; set; }
        public Categoria Categoria { get; set; }
        public decimal PrecoMedio { get; set; }

        public string UnidadeTexto => Unidade switch
        {
            UnidadeMedida.Kilo => "kg",
            UnidadeMedida.Grama => "g",
            UnidadeMedida.Litro => "L",
            UnidadeMedida.Unidade => "un",
            UnidadeMedida.Pacote => "pct",
            UnidadeMedida.Caixa => "cx",
            _ => "un"
        };

        public string CategoriaTexto => Categoria switch
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

        public string DisplayInfo => $"{PrecoMedio:C}/{UnidadeTexto} • {CategoriaTexto}";
    }
}