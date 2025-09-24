// TotaisManager.cs
using System.Globalization;

namespace ListaComprasApp.Services
{
    public static class TotaisManager
    {
        // Eventos para notificar mudanças
        public static event Action<string> TotalChanged;
        public static event Action<string> TotalCompradoChanged;

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
    }
}