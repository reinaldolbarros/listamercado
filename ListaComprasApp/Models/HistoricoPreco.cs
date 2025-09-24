using SQLite;

namespace ListaComprasApp.Models
{
    [Table("HistoricoPrecos")]
    public class HistoricoPreco
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [MaxLength(100)]
        public string NomeProduto { get; set; } = string.Empty;

        public decimal Preco { get; set; }

        public UnidadeMedida Unidade { get; set; }

        public DateTime DataRegistro { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string? Local { get; set; }
    }
}