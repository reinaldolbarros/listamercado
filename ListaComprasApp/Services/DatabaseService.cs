using SQLite;
using ListaComprasApp.Models;

namespace ListaComprasApp.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection? _database;

        private async Task Init()
        {
            if (_database is not null)
                return;

            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "ListaCompras.db");
            _database = new SQLiteAsyncConnection(databasePath);

            await _database.CreateTableAsync<ListaCompras>();
            await _database.CreateTableAsync<Item>();
            await _database.CreateTableAsync<HistoricoPreco>();
        }

        // Métodos para ListaCompras
        public async Task<List<ListaCompras>> GetListasAsync()
        {
            await Init();
            var listas = await _database!.Table<ListaCompras>()
                .OrderByDescending(l => l.DataCriacao)
                .ToListAsync();

            // Carregar itens para cada lista
            foreach (var lista in listas)
            {
                lista.Itens = await GetItensByListaAsync(lista.Id);
            }

            return listas;
        }

        public async Task<ListaCompras?> GetListaAsync(int id)
        {
            await Init();
            var lista = await _database!.GetAsync<ListaCompras>(id);
            if (lista != null)
            {
                lista.Itens = await GetItensByListaAsync(id);
            }
            return lista;
        }

        public async Task<int> SaveListaAsync(ListaCompras lista)
        {
            await Init();
            if (lista.Id != 0)
                return await _database!.UpdateAsync(lista);
            else
                return await _database!.InsertAsync(lista);
        }

        public async Task<int> DeleteListaAsync(ListaCompras lista)
        {
            await Init();
            // Deletar todos os itens da lista primeiro
            await _database!.ExecuteAsync("DELETE FROM Items WHERE ListaComprasId = ?", lista.Id);
            return await _database!.DeleteAsync(lista);
        }

        // Métodos para Itens
        public async Task<List<Item>> GetItensByListaAsync(int listaId)
        {
            await Init();
            return await _database!.Table<Item>()
                .Where(i => i.ListaComprasId == listaId)
                .OrderBy(i => i.Categoria)
                .ThenBy(i => i.Nome)
                .ToListAsync();
        }

        public async Task<int> SaveItemAsync(Item item)
        {
            await Init();

            // Salvar no histórico de preços
            if (item.ValorUnitario > 0)
            {
                var historico = new HistoricoPreco
                {
                    NomeProduto = item.Nome,
                    Preco = item.ValorUnitario,
                    Unidade = item.Unidade,
                    DataRegistro = DateTime.Now
                };
                await _database!.InsertAsync(historico);
            }

            if (item.Id != 0)
                return await _database!.UpdateAsync(item);
            else
                return await _database!.InsertAsync(item);
        }

        public async Task<int> DeleteItemAsync(Item item)
        {
            await Init();
            return await _database!.DeleteAsync(item);
        }

        public async Task<int> UpdateItemCompradoAsync(int itemId, bool comprado)
        {
            await Init();
            return await _database!.ExecuteAsync(
                "UPDATE Items SET Comprado = ? WHERE Id = ?",
                comprado, itemId);
        }

        // Métodos para Histórico de Preços
        public async Task<List<HistoricoPreco>> GetHistoricoPrecoAsync(string nomeProduto)
        {
            await Init();
            return await _database!.Table<HistoricoPreco>()
                .Where(h => h.NomeProduto.ToLower() == nomeProduto.ToLower())
                .OrderByDescending(h => h.DataRegistro)
                .Take(50) // Últimos 50 registros
                .ToListAsync();
        }

        public async Task<decimal> GetPrecoMedioAsync(string nomeProduto, UnidadeMedida unidade)
        {
            await Init();
            var precos = await _database!.Table<HistoricoPreco>()
                .Where(h => h.NomeProduto.ToLower() == nomeProduto.ToLower() && h.Unidade == unidade)
                .OrderByDescending(h => h.DataRegistro)
                .Take(10) // Últimos 10 preços
                .ToListAsync();

            return precos.Any() ? precos.Average(p => p.Preco) : 0;
        }

        // Métodos para Relatórios Premium
        public async Task<List<Item>> GetItensCompradosAsync(DateTime dataInicio, DateTime dataFim)
        {
            await Init();
            return await _database!.QueryAsync<Item>(
                @"SELECT i.* FROM Items i 
                  INNER JOIN ListasCompras l ON i.ListaComprasId = l.Id 
                  WHERE i.Comprado = 1 AND l.DataCompra >= ? AND l.DataCompra <= ?",
                dataInicio, dataFim);
        }

        public async Task<Dictionary<Categoria, decimal>> GetGastosPorCategoriaAsync(DateTime dataInicio, DateTime dataFim)
        {
            await Init();
            var itens = await GetItensCompradosAsync(dataInicio, dataFim);

            return itens.GroupBy(i => i.Categoria)
                       .ToDictionary(g => g.Key, g => g.Sum(i => i.ValorTotal));
        }

        // Método para buscar produtos similares (autocomplete)
        public async Task<List<string>> GetProdutosSimilaresAsync(string termo)
        {
            await Init();
            var produtos = await _database!.QueryAsync<HistoricoPreco>(
                "SELECT DISTINCT NomeProduto FROM HistoricoPrecos WHERE NomeProduto LIKE ? LIMIT 10",
                $"%{termo}%");

            return produtos.Select(p => p.NomeProduto).ToList();
        }
    }
}