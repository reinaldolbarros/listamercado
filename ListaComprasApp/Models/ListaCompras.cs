using SQLite;

namespace ListaComprasApp.Models
{
    [Table("ListasCompras")]
    public class ListaCompras
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [MaxLength(100)]
        public string Nome { get; set; } = string.Empty;

        public DateTime DataCriacao { get; set; } = DateTime.Now;

        public DateTime? DataCompra { get; set; }

        public bool Finalizada { get; set; } = false;

        public decimal OrcamentoTotal { get; set; }

        [Ignore]
        public List<Item> Itens { get; set; } = new List<Item>();

        [Ignore]
        public decimal ValorTotalCalculado => Itens?.Sum(i => i.ValorTotal) ?? 0;

        [Ignore]
        public int TotalItens => Itens?.Count ?? 0;

        [Ignore]
        public int ItensComprados => Itens?.Count(i => i.Comprado) ?? 0;

        [Ignore]
        public decimal PercentualOrcamento => OrcamentoTotal > 0 ? (ValorTotalCalculado / OrcamentoTotal) * 100 : 0;
    }
}