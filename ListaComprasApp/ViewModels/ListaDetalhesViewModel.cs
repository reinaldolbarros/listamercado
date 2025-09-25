using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ListaComprasApp.Models;
using ListaComprasApp.Services;
using System.Collections.ObjectModel;

namespace ListaComprasApp.ViewModels
{
    [QueryProperty(nameof(ListaId), "listaId")]
    public partial class ListaDetalhesViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;

        public ObservableCollection<Item> Itens { get; } = new();

        [ObservableProperty]
        ListaCompras? listaAtual;

        [ObservableProperty]
        int listaId;

        [ObservableProperty]
        string nomeItem = string.Empty;

        [ObservableProperty]
        decimal valorUnitario;

        [ObservableProperty]
        decimal quantidade = 1;

        [ObservableProperty]
        UnidadeMedida unidadeSelecionada = UnidadeMedida.Unidade;

        [ObservableProperty]
        Categoria categoriaSelecionada = Categoria.Outras;

        [ObservableProperty]
        bool usarValorTotal;

        [ObservableProperty]
        decimal valorTotalManual;

        public List<UnidadeMedida> Unidades { get; } = Enum.GetValues<UnidadeMedida>().ToList();
        public List<Categoria> Categorias { get; } = Enum.GetValues<Categoria>().ToList();

        public ListaDetalhesViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        async partial void OnListaIdChanged(int value)
        {
            await CarregarLista();
        }

        partial void OnNomeItemChanged(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                var (unidade, categoria, icone, precoMedio) = ProdutosPadraoService.ObterPadrao(value);
                UnidadeSelecionada = unidade;
                CategoriaSelecionada = categoria;
                ValorUnitario = precoMedio;
            }
        }

        [RelayCommand]
        async Task CarregarLista()
        {
            if (ListaId == 0) return;

            try
            {
                IsBusy = true;
                ListaAtual = await _databaseService.GetListaAsync(ListaId);

                if (ListaAtual != null)
                {
                    Title = ListaAtual.Nome;
                    Itens.Clear();
                    foreach (var item in ListaAtual.Itens)
                    {
                        Itens.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao carregar lista: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task AdicionarItem()
        {
            if (string.IsNullOrWhiteSpace(NomeItem))
            {
                await Shell.Current.DisplayAlert("Atenção", "Digite o nome do item", "OK");
                return;
            }

            if (!UsarValorTotal && (ValorUnitario <= 0 || Quantidade <= 0))
            {
                await Shell.Current.DisplayAlert("Atenção", "Digite valores válidos", "OK");
                return;
            }

            if (UsarValorTotal && ValorTotalManual <= 0)
            {
                await Shell.Current.DisplayAlert("Atenção", "Digite um valor total válido", "OK");
                return;
            }

            try
            {
                var (unidade, categoria, icone, precoMedio) = ProdutosPadraoService.ObterPadrao(NomeItem);

                var novoItem = new Item
                {
                    Nome = NomeItem,
                    Icone = icone,
                    Unidade = UnidadeSelecionada,
                    Categoria = CategoriaSelecionada,
                    Quantidade = Quantidade,
                    ValorUnitario = ValorUnitario,
                    ValorTotalManual = UsarValorTotal ? ValorTotalManual : null,
                    ListaComprasId = ListaId
                };

                await _databaseService.SaveItemAsync(novoItem);

                // Limpar campos
                NomeItem = string.Empty;
                ValorUnitario = 0;
                Quantidade = 1;
                ValorTotalManual = 0;
                UsarValorTotal = false;

                await CarregarLista();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao adicionar item: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        async Task MarcarComprado(Item item)
        {
            if (item == null) return;

            try
            {
                item.Comprado = !item.Comprado;
                await _databaseService.UpdateItemCompradoAsync(item.Id, item.Comprado);

                // Atualizar a lista para refletir mudanças
                await CarregarLista();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao atualizar item: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        async Task ExcluirItem(Item item)
        {
            if (item == null) return;

            bool confirmar = await Shell.Current.DisplayAlert(
                "Excluir Item",
                $"Deseja excluir '{item.Nome}'?",
                "Sim", "Não");

            if (!confirmar) return;

            try
            {
                await _databaseService.DeleteItemAsync(item);
                await CarregarLista();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao excluir item: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        async Task AbrirProdutosPadrao()
        {
            var produtos = ProdutosPadraoService.ObterTodosProdutos();
            var opcoes = produtos.Take(15).Select(p => $"{p.Icone} {p.Nome} - R$ {p.PrecoMedio:F2}").ToArray();

            var resultado = await Shell.Current.DisplayActionSheet(
                "Selecione um produto para adicionar:",
                "Cancelar",
                null,
                opcoes);

            if (resultado != "Cancelar" && !string.IsNullOrEmpty(resultado))
            {
                // Extrair o nome do produto da string selecionada
                var nomeProduto = resultado.Split(" - ")[0].Substring(2).Trim();
                var produtoSelecionado = produtos.First(p => p.Nome == nomeProduto);

                var novoItem = new Item
                {
                    Nome = produtoSelecionado.Nome,
                    Icone = produtoSelecionado.Icone,
                    Unidade = produtoSelecionado.Unidade,
                    Categoria = produtoSelecionado.Categoria,
                    ValorUnitario = produtoSelecionado.PrecoMedio,
                    Quantidade = 1,
                    ListaComprasId = ListaId
                };

                await _databaseService.SaveItemAsync(novoItem);
                await CarregarLista();

                await Shell.Current.DisplayAlert("Sucesso", $"{produtoSelecionado.Nome} adicionado à lista!", "OK");
            }
        }

        [RelayCommand]
        async Task FinalizarCompra()
        {
            if (ListaAtual == null) return;

            bool confirmar = await Shell.Current.DisplayAlert(
                "Finalizar Compra",
                "Deseja marcar esta lista como finalizada?",
                "Sim", "Não");

            if (!confirmar) return;

            try
            {
                ListaAtual.Finalizada = true;
                ListaAtual.DataCompra = DateTime.Now;
                await _databaseService.SaveListaAsync(ListaAtual);

                await Shell.Current.GoToAsync("//main");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao finalizar compra: {ex.Message}", "OK");
            }
        }
    }
}