using SQLite;

namespace ListaComprasApp.Models
{
    [Table("Items")]
    public class Item
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [MaxLength(100)]
        public string Nome { get; set; } = string.Empty;

        [MaxLength(10)]
        public string Icone { get; set; } = "📦";

        public UnidadeMedida Unidade { get; set; }

        public decimal ValorUnitario { get; set; }

        public decimal Quantidade { get; set; } = 1;

        public decimal? ValorTotalManual { get; set; }

        public Categoria Categoria { get; set; }

        public bool Comprado { get; set; } = false;

        public int ListaComprasId { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;

        [Ignore]
        public decimal ValorTotal => ValorTotalManual ?? (ValorUnitario * Quantidade);

        [Ignore]
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

        [Ignore]
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

        [Ignore]
        public string DisplayTexto => $"{Icone} {Nome}";
    }
}