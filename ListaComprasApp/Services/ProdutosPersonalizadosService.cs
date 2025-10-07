using System.Text.Json;
using System.Diagnostics;
using ListaComprasApp.Models;

namespace ListaComprasApp.Services
{
    public static class ProdutosPersonalizadosService
    {
        // Armazena as modificações dos produtos padrão (preços)
        private static Dictionary<string, decimal> _precosProdutos = new();

        // Armazena produtos adicionados pelo usuário
        private static Dictionary<string, (UnidadeMedida Unidade, Categoria Categoria, string Icone, decimal PrecoMedio)> _produtosAdicionados = new();

        // Armazena produtos que foram excluídos
        private static List<string> _produtosExcluidos = new();

        // Chaves para o Preferences
        private const string PRODUTOS_EXCLUIDOS_KEY = "ProdutosPadraoExcluidos";
        private const string PRODUTOS_ADICIONADOS_KEY = "ProdutosAdicionados";
        private const string PRECOS_PRODUTOS_KEY = "PrecosProdutos";

        static ProdutosPersonalizadosService()
        {
            CarregarDados();
        }

        // Carregar dados do armazenamento
        public static void CarregarDados()
        {
            try
            {
                // Carregar produtos excluídos
                string excluidosJson = Preferences.Get(PRODUTOS_EXCLUIDOS_KEY, "");
                if (!string.IsNullOrEmpty(excluidosJson))
                {
                    var excluidos = JsonSerializer.Deserialize<List<string>>(excluidosJson);
                    if (excluidos != null)
                        _produtosExcluidos = excluidos;
                }

                // Carregar preços modificados
                string precosJson = Preferences.Get(PRECOS_PRODUTOS_KEY, "");
                if (!string.IsNullOrEmpty(precosJson))
                {
                    var precos = JsonSerializer.Deserialize<Dictionary<string, decimal>>(precosJson);
                    if (precos != null)
                        _precosProdutos = precos;
                }

                // Carregar produtos adicionados
                string adicionadosJson = Preferences.Get(PRODUTOS_ADICIONADOS_KEY, "");
                if (!string.IsNullOrEmpty(adicionadosJson))
                {
                    var produtosSerializados = JsonSerializer.Deserialize<List<ProdutoSerializado>>(adicionadosJson);
                    if (produtosSerializados != null)
                    {
                        _produtosAdicionados.Clear();
                        foreach (var p in produtosSerializados)
                        {
                            _produtosAdicionados[p.Nome] = (p.Unidade, p.Categoria, p.Icone, p.PrecoMedio);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao carregar dados personalizados: {ex.Message}");
            }
        }

        // Salvar todos os dados no armazenamento
        private static void SalvarDados()
        {
            try
            {
                // Salvar produtos excluídos
                string excluidosJson = JsonSerializer.Serialize(_produtosExcluidos);
                Preferences.Set(PRODUTOS_EXCLUIDOS_KEY, excluidosJson);

                // Salvar preços modificados
                string precosJson = JsonSerializer.Serialize(_precosProdutos);
                Preferences.Set(PRECOS_PRODUTOS_KEY, precosJson);

                // Salvar produtos adicionados (conversão para uma lista serializável)
                var produtosSerializados = _produtosAdicionados.Select(p => new ProdutoSerializado
                {
                    Nome = p.Key,
                    Unidade = p.Value.Unidade,
                    Categoria = p.Value.Categoria,
                    Icone = p.Value.Icone,
                    PrecoMedio = p.Value.PrecoMedio
                }).ToList();

                string adicionadosJson = JsonSerializer.Serialize(produtosSerializados);
                Preferences.Set(PRODUTOS_ADICIONADOS_KEY, adicionadosJson);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao salvar dados personalizados: {ex.Message}");
            }
        }

        // Verificar se um produto está excluído
        public static bool EstaProdutoExcluido(string nome)
        {
            return _produtosExcluidos.Contains(nome);
        }

        // Excluir um produto
        public static bool ExcluirProduto(string nome)
        {
            if (!_produtosExcluidos.Contains(nome))
            {
                _produtosExcluidos.Add(nome);
                SalvarDados();
                return true;
            }
            return false;
        }

        // Restaurar um produto excluído
        public static bool RestaurarProduto(string nome)
        {
            if (_produtosExcluidos.Contains(nome))
            {
                _produtosExcluidos.Remove(nome);
                SalvarDados();
                return true;
            }
            return false;
        }

        // Adicionar um novo produto
        public static bool AdicionarProduto(string nome, UnidadeMedida unidade, Categoria categoria, string icone, decimal precoMedio)
        {
            var nomeLower = nome.ToLower().Trim();

            // Verificar se já existe nos produtos padrão originais
            if (ProdutosPadraoService.ProdutosPadrao.ContainsKey(nomeLower))
            {
                return false;
            }

            // Verificar se já existe nos produtos adicionados
            if (_produtosAdicionados.ContainsKey(nomeLower))
            {
                return false;
            }

            // Adicionar o novo produto
            _produtosAdicionados[nomeLower] = (unidade, categoria, icone, precoMedio);

            // Se estava na lista de excluídos, remover
            if (_produtosExcluidos.Contains(nomeLower))
            {
                _produtosExcluidos.Remove(nomeLower);
            }

            SalvarDados();
            return true;
        }

        // Atualizar o preço de um produto
        public static bool AtualizarPrecoProduto(string nome, decimal novoPreco)
        {
            var nomeLower = nome.ToLower().Trim();

            // Se está nos produtos adicionados, atualiza diretamente
            if (_produtosAdicionados.ContainsKey(nomeLower))
            {
                var produto = _produtosAdicionados[nomeLower];
                _produtosAdicionados[nomeLower] = (produto.Unidade, produto.Categoria, produto.Icone, novoPreco);
                SalvarDados();
                return true;
            }

            // Se está nos produtos padrão, salva o preço modificado
            if (ProdutosPadraoService.ProdutosPadrao.ContainsKey(nomeLower))
            {
                _precosProdutos[nomeLower] = novoPreco;
                SalvarDados();
                return true;
            }

            return false;
        }

        // Obter todos os produtos personalizados
        public static List<(string Nome, UnidadeMedida Unidade, Categoria Categoria, string Icone, decimal PrecoMedio)> ObterProdutosPersonalizados()
        {
            return _produtosAdicionados.Select(p => (
                Nome: p.Key,
                Unidade: p.Value.Unidade,
                Categoria: p.Value.Categoria,
                Icone: p.Value.Icone,
                PrecoMedio: p.Value.PrecoMedio
            )).ToList();
        }

        // Obter o preço personalizado de um produto
        public static decimal? ObterPrecoPersonalizado(string nome)
        {
            var nomeLower = nome.ToLower().Trim();

            if (_precosProdutos.ContainsKey(nomeLower))
            {
                return _precosProdutos[nomeLower];
            }

            return null;
        }

        // Classe auxiliar para serialização
        private class ProdutoSerializado
        {
            public string Nome { get; set; } = string.Empty;
            public UnidadeMedida Unidade { get; set; }
            public Categoria Categoria { get; set; }
            public string Icone { get; set; } = string.Empty;
            public decimal PrecoMedio { get; set; }
        }
    }
}