using MySqlConnector;
using Sakkola.Models.ViewProdutos;
using Sakkola.Repository.Interfaces;

namespace Sakkola.Repository
{
    public class ProdutoRepository : IProdutoRepository
    {
        private readonly string _connectionString;

        public ProdutoRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("conexaoMySQL")!;
        }

        public async Task<List<Produtos>> ObterTodosAsync()
        {
            var produtos = new List<Produtos>();
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string sql = @"
                    SELECT p.id_produto, p.nome, p.descricao, p.preco, p.estoque, p.id_categoria, c.nome AS nome_categoria
                    FROM tbProduto p
                    INNER JOIN tbCategoria c ON p.id_categoria = c.id_categoria
                    ORDER BY p.id_produto DESC;";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            produtos.Add(MapearProduto(reader));
                        }
                    }
                }
            }
            return produtos;
        }

        public async Task<List<Produtos>> ObterPorCategoriaAsync(string nomeCategoria)
        {
            var produtos = new List<Produtos>();
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string sql = @"
                    SELECT p.id_produto, p.nome, p.descricao, p.preco, p.estoque, p.id_categoria, c.nome AS nome_categoria
                    FROM tbProduto p
                    INNER JOIN tbCategoria c ON p.id_categoria = c.id_categoria
                    WHERE LOWER(c.nome) LIKE LOWER(CONCAT('%', @nomeCategoria, '%'))
                    ORDER BY p.nome ASC;";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@nomeCategoria", nomeCategoria ?? string.Empty);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            produtos.Add(MapearProduto(reader));
                        }
                    }
                }
            }
            return produtos;
        }

        public async Task<Produtos> ObterPorIdAsync(int id)
        {
            Produtos produto = null;
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string sql = @"
                    SELECT p.id_produto, p.nome, p.descricao, p.preco, p.estoque, p.id_categoria, c.nome AS nome_categoria
                    FROM tbProduto p
                    INNER JOIN tbCategoria c ON p.id_categoria = c.id_categoria
                    WHERE p.id_produto = @id;";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            produto = MapearProduto(reader);
                        }
                    }
                }
            }
            return produto;
        }

        public async Task<List<Produtos>> ObterPorCategoriaExcetoIdAsync(int idCategoria, int idProdutoAtual, int limite = 4)
        {
            var produtos = new List<Produtos>();
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string sql = @"
                    SELECT p.id_produto, p.nome, p.descricao, p.preco, p.estoque, p.id_categoria, c.nome AS nome_categoria
                    FROM tbProduto p
                    INNER JOIN tbCategoria c ON p.id_categoria = c.id_categoria
                    WHERE p.id_categoria = @idCategoria AND p.id_produto != @idProdutoAtual
                    LIMIT @limite;";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@idCategoria", idCategoria);
                    cmd.Parameters.AddWithValue("@idProdutoAtual", idProdutoAtual);
                    cmd.Parameters.AddWithValue("@limite", limite);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            produtos.Add(MapearProduto(reader));
                        }
                    }
                }
            }
            return produtos;
        }

        private Produtos MapearProduto(MySqlDataReader reader)
        {
            return new Produtos
            {
                id = reader.GetInt32("id_produto"),
                nome = reader.GetString("nome"),
                descricao = reader.IsDBNull(reader.GetOrdinal("descricao")) ? string.Empty : reader.GetString("descricao"),
                preco = reader.GetDecimal("preco"),
                estoque = reader.GetInt32("estoque"),
                id_categoria = reader.GetInt32("id_categoria"),
                Categoria = new Categoria
                {
                    id = reader.GetInt32("id_categoria"),
                    categoria = reader.GetString("nome_categoria")
                }
            };
        }
    }
}