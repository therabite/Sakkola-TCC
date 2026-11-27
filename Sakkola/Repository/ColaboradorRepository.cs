using MySqlConnector;
using Sakkola.Models;
using Sakkola.Models.ViewModel;
using Sakkola.Models.ViewProdutos;
using Sakkola.Repository.Interfaces;

namespace Sakkola.Repository
{
    public class ColaboradorRepository : IColaboradorRepository
    {
        private readonly string _connectionString;

        public ColaboradorRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("conexaoMySQL");
        }
        public async Task<Colaborador> ValidarColaboradorAsync(string email, string senha)
        {
            using (var conexao = new MySqlConnection(_connectionString))
            {
                await conexao.OpenAsync();
                string sql = @"
                    SELECT f.id_employed, f.is_admin, u.nome, u.email, u.senha 
                    FROM tbfuncionario f
                    INNER JOIN tbuser u ON f.id_employed = u.id_user
                    WHERE u.email = @Email AND u.senha = @Senha;";

                using (var cmd = new MySqlCommand(sql, conexao))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Senha", senha);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Colaborador
                            {
                                Id_colaborador = reader.GetInt32("id_employed"),
                                Nome = reader.GetString("nome"),
                                Email = reader.GetString("email"),
                                Senha = reader.GetString("senha"),
                                Administrador = reader.GetBoolean("is_admin")
                            };
                        }
                    }
                }
            }
            return null;
        }
        public async Task<List<Pedido>> ObterPedidosPendentesAsync()
        {
            var pedidos = new List<Pedido>();

            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string sql = "SELECT * FROM tbPedido ORDER BY data_pedido DESC;";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        int ordinalData = reader.GetOrdinal("data_pedido");

                        while (await reader.ReadAsync())
                        {
                            DateTime dataPedido = DateTime.MinValue;
                            if (!await reader.IsDBNullAsync(ordinalData))
                            {
                                try
                                {
                                    dataPedido = reader.GetDateTime(ordinalData);
                                }
                                catch
                                {
                                    if (DateTime.TryParse(reader[ordinalData]?.ToString(), out var parsed))
                                    {
                                        dataPedido = parsed;
                                    }
                                }
                            }

                            pedidos.Add(new Pedido
                            {
                                IdPedido = Convert.ToInt32(reader["id_pedido"]),
                                IdCliente = Convert.ToInt32(reader["id_user"]),
                                ValorTotal = Convert.ToDecimal(reader["valor_total"]),
                                DataPedido = dataPedido,
                                StatusPedido = reader["status"]?.ToString() ?? "Pendente"
                            });
                        }
                    }
                }
            }
            return pedidos;
        }
        public async Task CadastrarPedidoAsync(Pedido pedido)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string sql = "INSERT INTO tbPedido (id_user, valor_total, data_pedido, status) VALUES (@idUser, @valorTotal, NOW(), 'Pendente');";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@idUser", pedido.IdCliente);
                    cmd.Parameters.AddWithValue("@valorTotal", pedido.ValorTotal);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
        public async Task AtualizarStatusPedidoAsync(int idPedido, string novoStatus)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string sql = "UPDATE tbPedido SET status = @status WHERE id_pedido = @idPedido;";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@status", novoStatus);
                    cmd.Parameters.AddWithValue("@idPedido", idPedido);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
        public async Task CadastrarFuncionarioAsync(RegisterColaborador colaborador)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (var transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        string sqlUser = @"INSERT INTO tbuser (nome, data_nasc, telefone, CPF, email, senha) 
                                   VALUES (@nome, @dataNasc, @telefone, @cpf, @email, @senha);
                                   SELECT LAST_INSERT_ID();";

                        long idGerado;

                        using (var cmdUser = new MySqlCommand(sqlUser, conn, transaction))
                        {
                            cmdUser.Parameters.AddWithValue("@nome", colaborador.Nome);
                            cmdUser.Parameters.AddWithValue("@dataNasc", colaborador.DataNasc);
                            cmdUser.Parameters.AddWithValue("@telefone", colaborador.Telefone);
                            cmdUser.Parameters.AddWithValue("@cpf", colaborador.Cpf);
                            cmdUser.Parameters.AddWithValue("@email", colaborador.Email);
                            cmdUser.Parameters.AddWithValue("@senha", colaborador.Senha);

                            idGerado = Convert.ToInt64(await cmdUser.ExecuteScalarAsync());
                        }

                        string sqlFuncionario = "INSERT INTO tbFuncionario (id_employed, is_admin) VALUES (@idEmployed, @isAdmin);";

                        using (var cmdFunc = new MySqlCommand(sqlFuncionario, conn, transaction))
                        {
                            cmdFunc.Parameters.AddWithValue("@idEmployed", idGerado);
                            cmdFunc.Parameters.AddWithValue("@isAdmin", colaborador.IsAdmin);
                            await cmdFunc.ExecuteNonQueryAsync();
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
        public async Task<IEnumerable<string>> ObterCategoriasAsync()
        {
            var lista = new List<string>();

            using (var conexao = new MySqlConnection(_connectionString))
            {
                await conexao.OpenAsync();
                string sql = "SELECT nome FROM tbCategoria ORDER BY nome ASC;";

                using (var cmd = new MySqlCommand(sql, conexao))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            lista.Add(reader.GetString("nome"));
                        }
                    }
                }
            }
            return lista;
        }
        public async Task<int> InserirProdutoAsync(ProdutoCadastrarViewModel model)
        {
            int idGerado = 0;

            using (var conexao = new MySqlConnection(_connectionString))
            {
                await conexao.OpenAsync();

                string sql = "CALL sp_InserirProduto(@Nome, @Descricao, @Preco, @Estoque, @Categoria); SELECT LAST_INSERT_ID();";

                using (var cmd = new MySqlCommand(sql, conexao))
                {
                    cmd.Parameters.AddWithValue("@Nome", model.Nome);
                    cmd.Parameters.AddWithValue("@Descricao", model.Descricao);
                    cmd.Parameters.AddWithValue("@Preco", model.Preco);
                    cmd.Parameters.AddWithValue("@Estoque", model.Estoque);
                    cmd.Parameters.AddWithValue("@Categoria", model.Categoria);

                    var resultado = await cmd.ExecuteScalarAsync();
                    if (resultado != null)
                    {
                        idGerado = Convert.ToInt32(resultado);
                    }
                }
            }
            return idGerado;
        }
        public async Task<IEnumerable<Produtos>> ObterTodosProdutosAsync()
        {
            var lista = new List<Produtos>();
            using (var conexao = new MySqlConnection(_connectionString))
            {
                await conexao.OpenAsync();
                string sql = "SELECT id_produto, nome, descricao, preco, estoque, id_categoria FROM tbProduto;";

                using (var cmd = new MySqlCommand(sql, conexao))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            lista.Add(new Produtos
                            {
                                id = reader.GetInt32("id_produto"),
                                nome = reader.GetString("nome"),
                                descricao = reader.GetString("descricao"),
                                preco = reader.GetDecimal("preco"),
                                estoque = reader.GetInt32("estoque"),
                                id_categoria = reader.GetInt32("id_categoria")
                            });
                        }
                    }
                }
            }
            return lista;
        }
        public async Task DeletarProdutoAsync(int id)
        {
            using (var conexao = new MySqlConnection(_connectionString))
            {
                await conexao.OpenAsync();
                string sql = "DELETE FROM tbProduto WHERE id_produto = @Id;";

                using (var cmd = new MySqlCommand(sql, conexao))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
        public async Task AtualizarProdutoAsync(int id, ProdutoCadastrarViewModel model)
        {
            using (var conexao = new MySqlConnection(_connectionString))
            {
                await conexao.OpenAsync();
                string sql = @"
                    UPDATE tbProduto p
                    JOIN tbCategoria c ON c.nome = @Categoria
                    SET p.nome = @Nome, 
                        p.descricao = @Descricao, 
                        p.preco = @Preco, 
                        p.estoque = @Estoque, 
                        p.id_categoria = c.id_categoria
                    WHERE p.id_produto = @Id;";

                using (var cmd = new MySqlCommand(sql, conexao))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@Nome", model.Nome);
                    cmd.Parameters.AddWithValue("@Descricao", model.Descricao);
                    cmd.Parameters.AddWithValue("@Preco", model.Preco);
                    cmd.Parameters.AddWithValue("@Estoque", model.Estoque);
                    cmd.Parameters.AddWithValue("@Categoria", model.Categoria);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
        public async Task<ProdutoCadastrarViewModel> ObterProdutoPorIdAsync(int id)
        {
            using (var conexao = new MySqlConnection(_connectionString))
            {
                await conexao.OpenAsync();
                string sql = @"
                    SELECT p.nome, p.descricao, p.preco, p.estoque, c.nome AS categoria 
                    FROM tbProduto p
                    INNER JOIN tbCategoria c ON p.id_categoria = c.id_categoria
                    WHERE p.id_produto = @Id;";

                using (var cmd = new MySqlCommand(sql, conexao))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new ProdutoCadastrarViewModel
                            {
                                Nome = reader.GetString("nome"),
                                Descricao = reader.GetString("descricao"),
                                Preco = reader.GetDecimal("preco"),
                                Estoque = reader.GetInt32("estoque"),
                                Categoria = reader.GetString("categoria")
                            };
                        }
                    }
                }
            }
            return null;
        }
    }
}
