using Microsoft.EntityFrameworkCore;
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
            int idUsuario = int.Parse(userIdStr);
            var carrinho = new Carrinho();

            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                int idCarrinho = await ObterOuCriarCarrinhoIdAsync(conn, null, idUsuario);

                string sqlUsuario = @"
                    SELECT u.nome, u.telefone, 
                    e.cep, e.logradouro, e.numero, e.bairro, e.cidade, e.estado
                    FROM tbUser u
                    LEFT JOIN tbAddress e ON u.id_user = e.id_address
                    WHERE u.id_user = @idUsuario;";

                using (var cmdUser = new MySqlCommand(sqlUsuario, conn))
                {
                    cmdUser.Parameters.AddWithValue("@idUsuario", idUsuario);
                    using (var readerUser = await cmdUser.ExecuteReaderAsync())
                    {
                        if (await readerUser.ReadAsync())
                        {
                            carrinho.NomeCliente = readerUser.IsDBNull(readerUser.GetOrdinal("nome")) ? string.Empty : readerUser.GetString("nome");
                            carrinho.Telefone = readerUser.IsDBNull(readerUser.GetOrdinal("telefone")) ? string.Empty : readerUser.GetString("telefone");
                            carrinho.Cep = readerUser.IsDBNull(readerUser.GetOrdinal("cep")) ? string.Empty : readerUser.GetString("cep");
                            carrinho.Logradouro = readerUser.IsDBNull(readerUser.GetOrdinal("logradouro")) ? string.Empty : readerUser.GetString("logradouro");
                            carrinho.Numero = readerUser.IsDBNull(readerUser.GetOrdinal("numero")) ? string.Empty : readerUser.GetString("numero");
                            carrinho.Bairro = readerUser.IsDBNull(readerUser.GetOrdinal("bairro")) ? string.Empty : readerUser.GetString("bairro");
                            carrinho.Cidade = readerUser.IsDBNull(readerUser.GetOrdinal("cidade")) ? string.Empty : readerUser.GetString("cidade");
                            carrinho.Estado = readerUser.IsDBNull(readerUser.GetOrdinal("estado")) ? string.Empty : readerUser.GetString("estado");
                        }
                    }
                }

                // 2. Buscar os itens do carrinho (seu código original)
                string sqlItens = @"
                    SELECT ci.id_item, ci.id_produto, p.nome, p.preco, ci.quantidade 
                    FROM tbCarrinhoItem ci
                    INNER JOIN tbProduto p ON ci.id_produto = p.id_produto
                    WHERE ci.id_carrinho = @idCarrinho;";

                using (var cmdItens = new MySqlCommand(sqlItens, conn))
                {
                    cmdItens.Parameters.AddWithValue("@idCarrinho", idCarrinho);
                    using (var reader = await cmdItens.ExecuteReaderAsync())
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
        public async Task AtualizarQuantidadeItemAsync(int idItem, int quantidade)
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
        public async Task AtualizarDadosEntregaAsync(string userIdStr, Carrinho carrinho)
        {
            int idUsuario = int.Parse(userIdStr);

            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var tran = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        // 1. Atualizar Nome e Telefone na tabela de usuário
                        string sqlUsuario = "UPDATE tbUser SET nome = @nome, telefone = @telefone WHERE id_user = @idUsuario;";
                        using (var cmdUser = new MySqlCommand(sqlUsuario, conn, tran))
                        {
                            cmdUser.Parameters.AddWithValue("@nome", carrinho.NomeCliente ?? (object)DBNull.Value);
                            cmdUser.Parameters.AddWithValue("@telefone", carrinho.Telefone ?? (object)DBNull.Value);
                            cmdUser.Parameters.AddWithValue("@idUsuario", idUsuario);
                            await cmdUser.ExecuteNonQueryAsync();
                        }

                        // 2. Verificar se o endereço já existe para este usuário
                        string sqlCheckEnd = "SELECT COUNT(1) FROM tbAddress WHERE id_address = @idUsuario;";
                        long enderecoExiste = 0;
                        using (var cmdCheck = new MySqlCommand(sqlCheckEnd, conn, tran))
                        {
                            cmdCheck.Parameters.AddWithValue("@idUsuario", idUsuario);
                            enderecoExiste = Convert.ToInt64(await cmdCheck.ExecuteScalarAsync());
                        }

                        if (enderecoExiste > 0)
                        {
                            // Atualiza o endereço existente
                            string sqlUpdateEnd = @"
                            UPDATE tbAddress 
                            SET cep = @cep, logradouro = @logradouro, numero = @numero, 
                                bairro = @bairro, cidade = @cidade, estado = @estado 
                            WHERE id_address = @idUsuario;";

                            using (var cmdUpEnd = new MySqlCommand(sqlUpdateEnd, conn, tran))
                            {
                                cmdUpEnd.Parameters.AddWithValue("@cep", carrinho.Cep ?? (object)DBNull.Value);
                                cmdUpEnd.Parameters.AddWithValue("@logradouro", carrinho.Logradouro ?? (object)DBNull.Value);
                                cmdUpEnd.Parameters.AddWithValue("@numero", carrinho.Numero ?? (object)DBNull.Value);
                                cmdUpEnd.Parameters.AddWithValue("@bairro", carrinho.Bairro ?? (object)DBNull.Value);
                                cmdUpEnd.Parameters.AddWithValue("@cidade", carrinho.Cidade ?? (object)DBNull.Value);
                                cmdUpEnd.Parameters.AddWithValue("@estado", carrinho.Estado ?? (object)DBNull.Value);
                                cmdUpEnd.Parameters.AddWithValue("@idUsuario", idUsuario);
                                await cmdUpEnd.ExecuteNonQueryAsync();
                            }
                        }
                        else
                        {
                            // Insere um novo endereço caso não exista
                            string sqlInsEnd = @"
                            INSERT INTO tbAddress (id_address, cep, logradouro, numero, bairro, cidade, estado) 
                            VALUES (@idUsuario, @cep, @logradouro, @numero, @bairro, @cidade, @estado);";

                            using (var cmdInsEnd = new MySqlCommand(sqlInsEnd, conn, tran))
                            {
                                cmdInsEnd.Parameters.AddWithValue("@idUsuario", idUsuario);
                                cmdInsEnd.Parameters.AddWithValue("@cep", carrinho.Cep ?? (object)DBNull.Value);
                                cmdInsEnd.Parameters.AddWithValue("@logradouro", carrinho.Logradouro ?? (object)DBNull.Value);
                                cmdInsEnd.Parameters.AddWithValue("@numero", carrinho.Numero ?? (object)DBNull.Value);
                                cmdInsEnd.Parameters.AddWithValue("@bairro", carrinho.Bairro ?? (object)DBNull.Value);
                                cmdInsEnd.Parameters.AddWithValue("@cidade", carrinho.Cidade ?? (object)DBNull.Value);
                                cmdInsEnd.Parameters.AddWithValue("@estado", carrinho.Estado ?? (object)DBNull.Value);
                                await cmdInsEnd.ExecuteNonQueryAsync();
                            }
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
        public async Task<List<ItemCarrinho>> ObterItensPorUsuarioAsync(int idUser)
        {
            var itens = new List<ItemCarrinho>();

            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string sql = @"SELECT c.id_carrinho, c.id_produto, c.quantidade, 
                                  p.nome AS nome_produto, p.preco AS preco_unitario 
                           FROM tbCarrinho c
                           INNER JOIN tbProduto p ON c.id_produto = p.id_produto
                           WHERE c.id_user = @id_user;";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id_user", idUser);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            itens.Add(new ItemCarrinho
                            {
                                IdCarrinho = Convert.ToInt32(reader["id_carrinho"]),
                                IdProduto = Convert.ToInt32(reader["id_produto"]),
                                NomeProduto = reader["nome_produto"]?.ToString() ?? "",
                                PrecoUnitario = Convert.ToDecimal(reader["preco_unitario"]),
                                Quantidade = Convert.ToInt32(reader["quantidade"])
                            });
                        }
                    }
                }
            }

            return itens;
        }
        public async Task LimparCarrinhoAsync(int idUser)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string sql = "DELETE FROM tbCarrinho WHERE id_user = @id_user;";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id_user", idUser);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
        public async Task FinalizarCompraAsync(int idUser, decimal valorTotal)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (var transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        string sqlPedido = "INSERT INTO tbpedido (id_user, valor_total, data_pedido, status) VALUES (@idUser, @valorTotal, NOW(), 'Pendente');";

                        using (var cmdPedido = new MySqlCommand(sqlPedido, conn, transaction))
                        {
                            cmdPedido.Parameters.AddWithValue("@idUser", idUser);
                            cmdPedido.Parameters.AddWithValue("@valorTotal", valorTotal);
                            await cmdPedido.ExecuteNonQueryAsync();
                        }

                        string sqlLimparCarrinho = "DELETE FROM tbCarrinho WHERE id_user = @idUser;";

                        using (var cmdLimpar = new MySqlCommand(sqlLimparCarrinho, conn, transaction))
                        {
                            cmdLimpar.Parameters.AddWithValue("@idUser", idUser);
                            await cmdLimpar.ExecuteNonQueryAsync();
                        }

                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }
    }
}