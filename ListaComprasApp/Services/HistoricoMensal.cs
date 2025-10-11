// Adicione esta classe em um novo arquivo HistoricoMensal.cs
using System.Text.Json;

namespace ListaComprasApp.Services
{
    public static class HistoricoMensal
    {
        private const string PREF_KEY = "HistoricoMensal";

        // Estrutura: Mês-Ano -> { NomeProduto -> (Quantidade, Valor) }
        private static Dictionary<string, Dictionary<string, (int Quantidade, decimal Valor)>> _historico;

        static HistoricoMensal()
        {
            CarregarHistorico();
        }

        private static void CarregarHistorico()
        {
            string json = Preferences.Get(PREF_KEY, "");
            if (string.IsNullOrEmpty(json))
            {
                _historico = new Dictionary<string, Dictionary<string, (int, decimal)>>();
                return;
            }

            try
            {
                var data = JsonSerializer.Deserialize<SerializableHistorico>(json);
                _historico = new Dictionary<string, Dictionary<string, (int, decimal)>>();

                if (data != null)
                {
                    foreach (var mes in data.Meses)
                    {
                        _historico[mes.Chave] = new Dictionary<string, (int, decimal)>();

                        foreach (var item in mes.Itens)
                        {
                            _historico[mes.Chave][item.Nome] = (item.Quantidade, item.Valor);
                        }
                    }
                }
            }
            catch
            {
                _historico = new Dictionary<string, Dictionary<string, (int, decimal)>>();
            }
        }

        private static void SalvarHistorico()
        {
            var serializable = new SerializableHistorico
            {
                Meses = new List<MesHistorico>()
            };

            foreach (var mes in _historico)
            {
                var mesData = new MesHistorico
                {
                    Chave = mes.Key,
                    Itens = new List<ItemHistorico>()
                };

                foreach (var item in mes.Value)
                {
                    mesData.Itens.Add(new ItemHistorico
                    {
                        Nome = item.Key,
                        Quantidade = item.Value.Quantidade,
                        Valor = item.Value.Valor
                    });
                }

                serializable.Meses.Add(mesData);
            }

            string json = JsonSerializer.Serialize(serializable);
            Preferences.Set(PREF_KEY, json);
        }

        // Para finalizar compra (soma quantidades)
        public static void AdicionarItem(DateTime data, string nome, int quantidade, decimal valor)
        {
            string chave = $"{data.Year}-{data.Month}";

            if (!_historico.ContainsKey(chave))
            {
                _historico[chave] = new Dictionary<string, (int, decimal)>();
            }

            if (_historico[chave].ContainsKey(nome))
            {
                var atual = _historico[chave][nome];
                _historico[chave][nome] = (atual.Quantidade + quantidade, valor);
            }
            else
            {
                _historico[chave][nome] = (quantidade, valor);
            }

            SalvarHistorico();
        }

        // Para editar meses passados (substitui valores)
        public static void SalvarItem(DateTime data, string nome, int quantidade, decimal valor)
        {
            string chave = $"{data.Year}-{data.Month}";

            if (!_historico.ContainsKey(chave))
            {
                _historico[chave] = new Dictionary<string, (int, decimal)>();
            }

            _historico[chave][nome] = (quantidade, valor);
            SalvarHistorico();
        }

        public static Dictionary<string, (int Quantidade, decimal Valor)> ObterHistoricoMes(DateTime data)
        {
            string chave = $"{data.Year}-{data.Month}";

            if (_historico.ContainsKey(chave))
            {
                return _historico[chave];
            }

            return new Dictionary<string, (int, decimal)>();
        }

        public static void RemoverItem(DateTime data, string nome)
        {
            string chave = $"{data.Year}-{data.Month}";

            if (_historico.ContainsKey(chave) && _historico[chave].ContainsKey(nome))
            {
                _historico[chave].Remove(nome);

                if (_historico[chave].Count == 0)
                {
                    _historico.Remove(chave);
                }

                SalvarHistorico();
            }
        }

        // Classes auxiliares para serialização
        private class SerializableHistorico
        {
            public List<MesHistorico> Meses { get; set; } = new List<MesHistorico>();
        }

        private class MesHistorico
        {
            public string Chave { get; set; } = "";
            public List<ItemHistorico> Itens { get; set; } = new List<ItemHistorico>();
        }

        private class ItemHistorico
        {
            public string Nome { get; set; } = "";
            public int Quantidade { get; set; }
            public decimal Valor { get; set; }
        }
    }
}