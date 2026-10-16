using MySqlConnector;
using Sakkola.Models.ViewCarrinho;
using Sakkola.Repository.Interfaces;

namespace Sakkola.Repository
{
    public class CarrinhoRepository : ICarrinhoRepository
    {
        private readonly string _connectionString;

        public CarrinhoRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("conexaoMySQL")!;
        }

        // Descobre ou cria o ID do carrinho do usuário atual
        private async Task<int> ObterOuCriarCarrinhoIdAsync(MySqlConnection conn, MySqlTransaction? tran, int idUsuario)
        {
            string sqlSelect = "SELECT id_carrinho FROM tbCarrinho WHERE id_usuario = @idUsuario;";
            using (var cmd = new MySqlCommand(sqlSelect, conn, tran))
            {
                cmd.Parameters.AddWithValue("@idUsuario", idUsuario);
                var result = await cmd.ExecuteScalarAsync();
                if (result != null)
                {
                    return Convert.ToInt32(result);
                }
            }

            // Se não existir carrinho para este usuário, cria um
            string sqlInsert = "INSERT INTO tbCarrinho (id_usuario) VALUES (@idUsuario); SELECT LAST_INSERT_ID();";
            using (var cmdInsert = new MySqlCommand(sqlInsert, conn, tran))
            {
                cmdInsert.Parameters.AddWithValue("@idUsuario", idUsuario);
                return Convert.ToInt32(await cmdInsert.ExecuteScalarAsync());
            }
        }

        public async Task<Carrinho> ObterCarrinhoPorUsuarioAsync(string userIdStr)
        {
            int idUsuario = int.Parse(userIdStr); // Converte o ID do claim para int (ajuste se seu ID for string/Guid)
            var carrinho = new Carrinho();

            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                int idCarrinho = await ObterOuCriarCarrinhoIdAsync(conn, null, idUsuario);

                string sql = @"
                    SELECT ci.id_item, ci.id_produto, p.nome, p.preco, ci.quantidade 
                    FROM tbCarrinhoItem ci
                    INNER JOIN tbProduto p ON ci.id_produto = p.id_produto
                    WHERE ci.id_carrinho = @idCarrinho;";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@idCarrinho", idCarrinho);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            carrinho.Itens.Add(new ItemCarrinho
                            {
                                IdItem = reader.GetInt32("id_item"),
                                IdProduto = reader.GetInt32("id_produto"),
                                NomeProduto = reader.GetString("nome"),
                                PrecoUnitario = reader.GetDecimal("preco"),
                                Quantidade = reader.GetInt32("quantidade")  
                            });
                        }
                    }
                }
            }
            return carrinho;
        }

        public async Task AdicionarItemAsync(string userIdStr, int idProduto, int quantidade)
        {
            int idUsuario = int.Parse(userIdStr);

            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var tran = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        int idCarrinho = await ObterOuCriarCarrinhoIdAsync(conn, tran, idUsuario);

                        // Verifica se o produto já existe no carrinho do usuário
                        string sqlCheck = "SELECT id_item, quantidade FROM tbCarrinhoItem WHERE id_carrinho = @idCarrinho AND id_produto = @idProduto;";
                        using (var cmdCheck = new MySqlCommand(sqlCheck, conn, tran))
                        {
                            cmdCheck.Parameters.AddWithValue("@idCarrinho", idCarrinho);
                            cmdCheck.Parameters.AddWithValue("@idProduto", idProduto);

                            using (var reader = await cmdCheck.ExecuteReaderAsync())
                            {
                                if (await reader.ReadAsync())
                                {
                                    int idItem = reader.GetInt32("id_item");
                                    int qtdAtual = reader.GetInt32("quantidade");
                                    reader.Close();

                                    string sqlUpdate = "UPDATE tbCarrinhoItem SET quantidade = @qtd WHERE id_item = @idItem;";
                                    using (var cmdUpdate = new MySqlCommand(sqlUpdate, conn, tran))
                                    {
                                        cmdUpdate.Parameters.AddWithValue("@qtd", qtdAtual + quantidade);
                                        cmdUpdate.Parameters.AddWithValue("@idItem", idItem);
                                        await cmdUpdate.ExecuteNonQueryAsync();
                                    }
                                    await tran.CommitAsync();
                                    return;
                                }
                            }
                        }

                        // Se não existe, insere novo item
                        string sqlInsert = "INSERT INTO tbCarrinhoItem (id_carrinho, id_produto, quantidade) VALUES (@idCarrinho, @idProduto, @quantidade);";
                        using (var cmdInsert = new MySqlCommand(sqlInsert, conn, tran))
                        {
                            cmdInsert.Parameters.AddWithValue("@idCarrinho", idCarrinho);
                            cmdInsert.Parameters.AddWithValue("@idProduto", idProduto);
                            cmdInsert.Parameters.AddWithValue("@quantidade", quantidade);
                            await cmdInsert.ExecuteNonQueryAsync();
                        }

                        await tran.CommitAsync();
                    }
                    catch
                    {
                        await tran.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        public async Task AtualizarQuantidadeAsync(string userIdStr, int idItem, int quantidade)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string sql = "UPDATE tbCarrinhoItem SET quantidade = @quantidade WHERE id_item = @idItem;";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@quantidade", quantidade);
                    cmd.Parameters.AddWithValue("@idItem", idItem);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task RemoverItemAsync(string userIdStr, int idItem)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string sql = "DELETE FROM tbCarrinhoItem WHERE id_item = @idItem;";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@idItem", idItem);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}