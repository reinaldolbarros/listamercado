using System.Globalization;
using ListaComprasApp.Models;

namespace ListaComprasApp.Services
{
    public static class TotaisManager
    {
        // Eventos para notificar mudanças
        public static event Action<string>? TotalChanged;
        public static event Action<string>? TotalCompradoChanged;

        // Valores atuais
        private static decimal _total;
        private static decimal _totalComprado;

        // Métodos para atualizar valores
        public static void UpdateTotal(decimal newTotal)
        {
            _total = newTotal;
            TotalChanged?.Invoke(string.Format(CultureInfo.GetCultureInfo("pt-BR"),
                "TOTAL PREVISTO:\nR$ {0:N2}", newTotal));
        }

        public static void UpdateTotalComprado(decimal newTotalComprado)
        {
            _totalComprado = newTotalComprado;
            TotalCompradoChanged?.Invoke(string.Format(CultureInfo.GetCultureInfo("pt-BR"),
                "TOTAL COMPRADO:\nR$ {0:N2}", newTotalComprado));
        }

        // Calcular total baseado nos itens e quantidades
        public static decimal CalcularTotal(Dictionary<string, int> quantidades, Dictionary<string, decimal> valores)
        {
            decimal total = 0;

            foreach (var item in quantidades)
            {
                var nomeProduto = item.Key;
                var quantidade = item.Value;

                if (valores.ContainsKey(nomeProduto))
                {
                    total += valores[nomeProduto] * quantidade;
                }
            }

            return total;
        }

        // Calcular total de itens selecionados/marcados
        public static decimal CalcularTotalSelecionado(
            Dictionary<string, int> quantidades,
            Dictionary<string, decimal> valores,
            Dictionary<string, bool> selecionados)
        {
            decimal total = 0;

            foreach (var select in selecionados)
            {
                if (select.Value) // Se o item está selecionado
                {
                    var nomeProduto = select.Key;

                    if (quantidades.ContainsKey(nomeProduto) && valores.ContainsKey(nomeProduto))
                    {
                        var quantidade = quantidades[nomeProduto];
                        var valor = valores[nomeProduto];

                        total += valor * quantidade;
                    }
                }
            }

            return total;
        }

        // Obter valores atuais
        public static decimal GetTotal() => _total;
        public static decimal GetTotalComprado() => _totalComprado;

        // Formatar valor como moeda
        public static string FormatarMoeda(decimal valor)
        {
            return string.Format(CultureInfo.GetCultureInfo("pt-BR"), "R$ {0:N2}", valor);
        }
    }
}