using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ListaComprasApp.Models;
using ListaComprasApp.Services;
using System.Collections.ObjectModel;

namespace ListaComprasApp.ViewModels
{
    public partial class MainViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;

        public ObservableCollection<ListaCompras> Listas { get; } = new();

        [ObservableProperty]
        ListaCompras? listaSelecionada;

        public MainViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            Title = "Minhas Listas";
        }

        [RelayCommand]
        async Task CarregarListas()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                var listas = await _databaseService.GetListasAsync();

                Listas.Clear();
                foreach (var lista in listas)
                {
                    Listas.Add(lista);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao carregar listas: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task CriarNovaLista()
        {
            string nomeLista = await Shell.Current.DisplayPromptAsync(
                "Nova Lista",
                "Digite o nome da lista:",
                placeholder: "Ex: Compras da semana");

            if (string.IsNullOrWhiteSpace(nomeLista))
                return;

            try
            {
                var novaLista = new ListaCompras
                {
                    Nome = nomeLista,
                    DataCriacao = DateTime.Now
                };

                await _databaseService.SaveListaAsync(novaLista);
                await CarregarListas();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao criar lista: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        async Task EditarLista(ListaCompras lista)
        {
            if (lista == null) return;

            string novoNome = await Shell.Current.DisplayPromptAsync(
                "Editar Lista",
                "Digite o novo nome:",
                initialValue: lista.Nome);

            if (string.IsNullOrWhiteSpace(novoNome) || novoNome == lista.Nome)
                return;

            try
            {
                lista.Nome = novoNome;
                await _databaseService.SaveListaAsync(lista);
                await CarregarListas();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao editar lista: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        async Task ExcluirLista(ListaCompras lista)
        {
            if (lista == null) return;

            bool confirmar = await Shell.Current.DisplayAlert(
                "Excluir Lista",
                $"Deseja realmente excluir a lista '{lista.Nome}'?\nTodos os itens serão perdidos.",
                "Sim", "Não");

            if (!confirmar) return;

            try
            {
                await _databaseService.DeleteListaAsync(lista);
                await CarregarListas();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao excluir lista: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        async Task AbrirLista(ListaCompras lista)
        {
            if (lista == null) return;

            ListaSelecionada = lista;
            await Shell.Current.GoToAsync($"//listadetalhes?listaId={lista.Id}");
        }
    }
}