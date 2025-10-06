using ListaComprasApp.Models;
using System.Diagnostics;

namespace ListaComprasApp.Services
{
    public static class ProdutosPadraoService
    {
        public static Dictionary<string, (UnidadeMedida Unidade, Categoria Categoria, string Icone, decimal PrecoMedio)> ProdutosPadrao = new()
        {
            // Frutas e Verduras
            { "banana", (UnidadeMedida.Kilo, Categoria.FrutasVerduras, "🍌", 4.50m) },
            { "maçã", (UnidadeMedida.Kilo, Categoria.FrutasVerduras, "🍎", 6.00m) },
            { "tomate", (UnidadeMedida.Kilo, Categoria.FrutasVerduras, "🍅", 5.80m) },
            { "cebola", (UnidadeMedida.Kilo, Categoria.FrutasVerduras, "🧅", 3.20m) },
            { "batata", (UnidadeMedida.Kilo, Categoria.FrutasVerduras, "🥔", 4.80m) },
            { "alface", (UnidadeMedida.Unidade, Categoria.FrutasVerduras, "🥬", 2.50m) },
            { "cenoura", (UnidadeMedida.Kilo, Categoria.FrutasVerduras, "🥕", 4.20m) },
            { "laranja", (UnidadeMedida.Kilo, Categoria.FrutasVerduras, "🍊", 3.80m) },
            { "limão", (UnidadeMedida.Kilo, Categoria.FrutasVerduras, "🍋", 4.50m) },
            { "abacaxi", (UnidadeMedida.Unidade, Categoria.FrutasVerduras, "🍍", 8.00m) },
            { "mamão", (UnidadeMedida.Kilo, Categoria.FrutasVerduras, "🥭", 5.50m) },
            { "pepino", (UnidadeMedida.Kilo, Categoria.FrutasVerduras, "🥒", 3.80m) },
            
            // Carnes
            { "carne moída", (UnidadeMedida.Kilo, Categoria.Carnes, "🥩", 18.90m) },
            { "frango", (UnidadeMedida.Kilo, Categoria.Carnes, "🍗", 12.50m) },
            { "peixe", (UnidadeMedida.Kilo, Categoria.Carnes, "🐟", 25.00m) },
            { "linguiça", (UnidadeMedida.Kilo, Categoria.Carnes, "🌭", 15.80m) },
            { "picanha", (UnidadeMedida.Kilo, Categoria.Carnes, "🥩", 45.00m) },
            { "bacon", (UnidadeMedida.Kilo, Categoria.Carnes, "🥓", 22.00m) },
            { "peito de frango", (UnidadeMedida.Kilo, Categoria.Carnes, "🍗", 14.90m) },
            
            // Laticínios
            { "leite", (UnidadeMedida.Litro, Categoria.Laticinios, "🥛", 4.50m) },
            { "queijo", (UnidadeMedida.Kilo, Categoria.Laticinios, "🧀", 35.00m) },
            { "iogurte", (UnidadeMedida.Unidade, Categoria.Laticinios, "🥛", 3.80m) },
            { "manteiga", (UnidadeMedida.Unidade, Categoria.Laticinios, "🧈", 8.50m) },
            { "requeijão", (UnidadeMedida.Unidade, Categoria.Laticinios, "🧀", 6.90m) },
            { "leite condensado", (UnidadeMedida.Unidade, Categoria.Laticinios, "🥛", 4.20m) },
            { "creme de leite", (UnidadeMedida.Unidade, Categoria.Laticinios, "🥛", 3.50m) },
            
            // Limpeza
            { "detergente", (UnidadeMedida.Unidade, Categoria.Limpeza, "🧽", 2.80m) },
            { "sabão em pó", (UnidadeMedida.Caixa, Categoria.Limpeza, "📦", 12.50m) },
            { "papel higiênico", (UnidadeMedida.Pacote, Categoria.Limpeza, "🧻", 15.90m) },
            { "desinfetante", (UnidadeMedida.Unidade, Categoria.Limpeza, "🧽", 4.50m) },
            { "água sanitária", (UnidadeMedida.Unidade, Categoria.Limpeza, "🧽", 3.20m) },
            { "sabonete", (UnidadeMedida.Unidade, Categoria.Limpeza, "🧼", 2.50m) },
            { "shampoo", (UnidadeMedida.Unidade, Categoria.Higiene, "🧴", 12.90m) },
            
            // Bebidas
            { "refrigerantesukitacocacola", (UnidadeMedida.Litro, Categoria.Bebidas, "🥤", 5.50m) },
            { "cerveja", (UnidadeMedida.Unidade, Categoria.Bebidas, "🍺", 3.80m) },
            { "água", (UnidadeMedida.Litro, Categoria.Bebidas, "💧", 2.50m) },
            { "suco", (UnidadeMedida.Litro, Categoria.Bebidas, "🧃", 4.90m) },
            { "café", (UnidadeMedida.Pacote, Categoria.Bebidas, "☕", 8.50m) },
            { "açúcar", (UnidadeMedida.Kilo, Categoria.ListaGeral, "🍯", 4.20m) },
            
            // Padaria
            { "pão", (UnidadeMedida.Unidade, Categoria.Padaria, "🍞", 0.50m) },
            { "bolo", (UnidadeMedida.Unidade, Categoria.Padaria, "🎂", 15.00m) },
            { "biscoito", (UnidadeMedida.Pacote, Categoria.Padaria, "🍪", 4.80m) },
            { "pão de açúcar", (UnidadeMedida.Unidade, Categoria.Padaria, "🥖", 6.50m) },
            
            // Congelados
            { "pizza congelada", (UnidadeMedida.Unidade, Categoria.Congelados, "🍕", 12.90m) },
            { "sorvete", (UnidadeMedida.Unidade, Categoria.Congelados, "🍦", 8.50m) },
            { "nuggets", (UnidadeMedida.Pacote, Categoria.Congelados, "🍗", 15.90m) },
            
            // Outros
            { "arroz", (UnidadeMedida.Kilo, Categoria.ListaGeral, "🍚", 6.50m) },
            { "feijão", (UnidadeMedida.Kilo, Categoria.ListaGeral, "🍲", 8.90m) },
            { "macarrão", (UnidadeMedida.Pacote, Categoria.ListaGeral, "🍝", 3.80m) },
            { "óleo", (UnidadeMedida.Litro, Categoria.ListaGeral, "🛢️", 7.50m) },
            { "sal", (UnidadeMedida.Kilo, Categoria.ListaGeral, "🧂", 2.20m) },
            { "farinha", (UnidadeMedida.Kilo, Categoria.ListaGeral, "🌾", 4.50m) }
        };

        public static (UnidadeMedida Unidade, Categoria Categoria, string Icone, decimal PrecoMedio) ObterPadrao(string nomeProduto)
        {
            var produtoLower = nomeProduto.ToLower().Trim();

            if (ProdutosPadrao.ContainsKey(produtoLower))
                return ProdutosPadrao[produtoLower];

            // Se não encontrar, retorna padrão
            return (UnidadeMedida.Unidade, Categoria.ListaGeral, "📦", 0.00m);
        }

        public static List<(string Nome, UnidadeMedida Unidade, Categoria Categoria, string Icone, decimal PrecoMedio)> ObterTodosProdutos()
        {
            return ProdutosPadrao.Select(p => (
                Nome: p.Key,
                Unidade: p.Value.Unidade,
                Categoria: p.Value.Categoria,
                Icone: p.Value.Icone,
                PrecoMedio: p.Value.PrecoMedio
            )).OrderBy(p => p.Categoria).ThenBy(p => p.Nome).ToList();
        }

        public static List<(string Nome, UnidadeMedida Unidade, Categoria Categoria, string Icone, decimal PrecoMedio)> ObterProdutosPorCategoria(Categoria categoria)
        {
            return ProdutosPadrao.Where(p => p.Value.Categoria == categoria)
                .Select(p => (
                    Nome: p.Key,
                    Unidade: p.Value.Unidade,
                    Categoria: p.Value.Categoria,
                    Icone: p.Value.Icone,
                    PrecoMedio: p.Value.PrecoMedio
                )).OrderBy(p => p.Nome).ToList();
        }

        // No ProdutosPadraoService.cs
        private static List<string> _produtosPadraoExcluidos = new();

        // Excluir um produto padrão
        public static bool ExcluirProdutoPadrao(string nomeProduto)
        {
            if (!_produtosPadraoExcluidos.Contains(nomeProduto))
            {
                _produtosPadraoExcluidos.Add(nomeProduto);
                SalvarProdutosExcluidos();
                return true;
            }
            return false;
        }

        // Método para salvar produtos excluídos
        private static void SalvarProdutosExcluidos()
        {
            try
            {
                string json = System.Text.Json.JsonSerializer.Serialize(_produtosPadraoExcluidos);
                Preferences.Set("ProdutosPadraoExcluidos", json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao salvar produtos excluídos: {ex.Message}");
            }
        }

        // Método para carregar produtos excluídos
        private static void CarregarProdutosExcluidos()
        {
            try
            {
                string json = Preferences.Get("ProdutosPadraoExcluidos", "");
                if (!string.IsNullOrEmpty(json))
                {
                    var excluidos = System.Text.Json.JsonSerializer.Deserialize<List<string>>(json);
                    if (excluidos != null)
                        _produtosPadraoExcluidos = excluidos;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao carregar produtos excluídos: {ex.Message}");
            }
        }

        // Modificar o método ObterTodosProdutos para filtrar produtos excluídos
        public static List<(string Nome, UnidadeMedida Unidade, Categoria Categoria, string Icone, decimal PrecoMedio)> ObterProdutosAtivos()
        {
            var produtosAtivos = new List<(string Nome, UnidadeMedida Unidade, Categoria Categoria, string Icone, decimal PrecoMedio)>();

            foreach (var produto in ObterTodosProdutos())
            {
                if (!_produtosPadraoExcluidos.Contains(produto.Nome))
                {
                    produtosAtivos.Add(produto);
                }
            }

            return produtosAtivos;
        }
        // Método para excluir um produto
        /// <summary>
        /// Exclui um produto da lista de produtos padrão
        /// </summary>
        /// <param name="nomeProduto">Nome do produto a ser excluído</param>
        /// <returns>True se o produto foi excluído com sucesso, False caso contrário</returns>
        public static bool ExcluirProduto(string nomeProduto)
        {
            try
            {
                // Obter a lista atual de produtos
                var produtos = ObterTodosProdutos().ToList();

                // Verificar se a lista está vazia
                if (produtos == null || produtos.Count == 0)
                    return false;

                // Encontrar o índice do produto a ser removido
                int indice = -1;
                for (int i = 0; i < produtos.Count; i++)
                {
                    if (produtos[i].Nome.Equals(nomeProduto, StringComparison.OrdinalIgnoreCase))
                    {
                        indice = i;
                        break;
                    }
                }

                // Se encontrou o produto, remova-o
                if (indice >= 0)
                {
                    produtos.RemoveAt(indice);

                    // Salvar a lista atualizada
                    string json = System.Text.Json.JsonSerializer.Serialize(produtos);
                    Preferences.Set("ProdutosPadrao", json);

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                // Registrar o erro, se ocorrer
                System.Diagnostics.Debug.WriteLine($"Erro ao excluir produto: {ex.Message}");
                return false;
            }
        }

        // Método para salvar a lista atualizada
        private static void SalvarListaProdutos(List<(string Nome, UnidadeMedida Unidade, Categoria Categoria, string Icone, decimal PrecoMedio)> produtos)
        {
            // Implementar conforme sua lógica de persistência
            // Exemplo:
            // string json = System.Text.Json.JsonSerializer.Serialize(produtos);
            // Preferences.Set("ProdutosPadrao", json);
        }

        public static bool AdicionarProdutoPadrao(string nome, UnidadeMedida unidade, Categoria categoria, string icone, decimal precoMedio)
        {
            var nomeLower = nome.ToLower().Trim();

            // Verificar se já existe
            if (ProdutosPadrao.ContainsKey(nomeLower))
            {
                return false; // Já existe
            }

            // Adicionar novo produto ao Dictionary
            ProdutosPadrao.Add(nomeLower, (unidade, categoria, icone, precoMedio));

            return true;
        }

        public static bool AtualizarPrecoPadrao(string nome, decimal novoPreco)
        {
            var nomeLower = nome.ToLower().Trim();

            if (ProdutosPadrao.ContainsKey(nomeLower))
            {
                var produto = ProdutosPadrao[nomeLower];
                ProdutosPadrao[nomeLower] = (produto.Unidade, produto.Categoria, produto.Icone, novoPreco);
                return true;
            }

            return false;
        }


    }
}