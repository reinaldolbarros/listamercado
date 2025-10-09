/* HistoricoComprasService.cs
using System.Text.Json;
using System.Diagnostics;
using ListaComprasApp.Models;

namespace ListaComprasApp.Services
{
    public static class HistoricoComprasService
    {
        private const string HISTORICO_KEY = "HistoricoComprasMensal";
        private static Dictionary<string, Dictionary<string, (int Quantidade, decimal Valor)>> _historico = new();

        static HistoricoComprasService()
        {
            CarregarHistorico();
        }

        public static void CarregarHistorico()
        {
            try
            {
                var json = Preferences.Get(HISTORICO_KEY, "");
                if (!string.IsNullOrEmpty(json))
                {
                    var dados = JsonSerializer.Deserialize<Dictionary<string, List<HistoricoItem>>>(json);
                    if (dados != null)
                    {
                        _historico = new Dictionary<string, Dictionary<string, (int, decimal)>>();
                        foreach (var mes in dados)
                        {
                            var itensMes = new Dictionary<string, (int, decimal)>();
                            foreach (var item in mes.Value)
                            {
                                itensMes[item.Nome] = (item.Quantidade, item.Valor);
                            }
                            _historico[mes.Key] = itensMes;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao carregar histórico: {ex.Message}");
            }
        }

        private static void SalvarHistorico()
        {
            try
            {
                var dados = new Dictionary<string, List<HistoricoItem>>();
                foreach (var mes in _historico)
                {
                    var itens = new List<HistoricoItem>();
                    foreach (var item in mes.Value)
                    {
                        itens.Add(new HistoricoItem
                        {
                            Nome = item.Key,
                            Quantidade = item.Value.Quantidade,
                            Valor = item.Value.Valor
                        });
                    }
                    dados[mes.Key] = itens;
                }

                var json = JsonSerializer.Serialize(dados);
                Preferences.Set(HISTORICO_KEY, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao salvar histórico: {ex.Message}");
            }
        }

        public static void AdicionarCompra(DateTime data, string produto, int quantidade, decimal valor)
        {
            var chave = $"{data.Year}-{data.Month}";

            if (!_historico.ContainsKey(chave))
            {
                _historico[chave] = new Dictionary<string, (int, decimal)>();
            }

            if (_historico[chave].ContainsKey(produto))
            {
                // Acumular a quantidade e usar o valor mais recente
                var qtdAtual = _historico[chave][produto].Quantidade;
                _historico[chave][produto] = (qtdAtual + quantidade, valor);
            }
            else
            {
                _historico[chave][produto] = (quantidade, valor);
            }

            SalvarHistorico();
        }

        public static Dictionary<string, (int Quantidade, decimal Valor)> ObterHistoricoMes(DateTime data)
        {
            var chave = $"{data.Year}-{data.Month}";
            if (_historico.ContainsKey(chave))
            {
                return _historico[chave];
            }
            return new Dictionary<string, (int, decimal)>();
        }

        public static bool ExisteHistoricoMes(DateTime data)
        {
            var chave = $"{data.Year}-{data.Month}";
            return _historico.ContainsKey(chave) && _historico[chave].Count > 0;
        }

        private class HistoricoItem
        {
            public string Nome { get; set; } = "";
            public int Quantidade { get; set; }
            public decimal Valor { get; set; }
        }
    }
}*/