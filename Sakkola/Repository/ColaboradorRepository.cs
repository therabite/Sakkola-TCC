using MySqlConnector;
using Sakkola.Models;
using Sakkola.Models.ViewModel;
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
        public async Task<LoginColaborador> ValidarColaboradorAsync(string email, string senha)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string sql = @"SELECT u.id_user, u.email, f.is_admin 
                       FROM tbFuncionario f
                       INNER JOIN tbUser u ON f.id_employed = u.id_user
                       WHERE u.email = @email AND u.senha = @senha;";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@senha", senha);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            bool isAdmin = Convert.ToBoolean(reader["is_admin"]);
                            if (!isAdmin)
                            {
                                return null;
                            }

                            return new LoginColaborador
                            {
                                Email = reader["email"].ToString(),
                                Senha = reader["senha"].ToString()
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
                        while (await reader.ReadAsync())
                        {
                            pedidos.Add(new Pedido
                            {
                                IdPedido = Convert.ToInt32(reader["id_pedido"]),
                                IdCliente = Convert.ToInt32(reader["id_user"]),
                                ValorTotal = Convert.ToDecimal(reader["valor_total"]),
                                DataPedido = Convert.ToDateTime(reader["data_pedido"]),
                                Status = reader["status"]?.ToString() ?? "Pendente"
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
        public async Task CadastrarFuncionarioAsync(string nome, DateTime dataNasc, int telefone, int cpf, string email, string senha, bool isAdmin)
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
                            cmdUser.Parameters.AddWithValue("@nome", nome);
                            cmdUser.Parameters.AddWithValue("@dataNasc", dataNasc);
                            cmdUser.Parameters.AddWithValue("@telefone", telefone);
                            cmdUser.Parameters.AddWithValue("@cpf", cpf);
                            cmdUser.Parameters.AddWithValue("@email", email);
                            cmdUser.Parameters.AddWithValue("@senha", senha);

                            idGerado = Convert.ToInt64(await cmdUser.ExecuteScalarAsync());
                        }

                        string sqlFuncionario = "INSERT INTO tbFuncionario (id_employed, is_admin) VALUES (@idEmployed, @isAdmin);";

                        using (var cmdFunc = new MySqlCommand(sqlFuncionario, conn, transaction))
                        {
                            cmdFunc.Parameters.AddWithValue("@idEmployed", idGerado);
                            cmdFunc.Parameters.AddWithValue("@isAdmin", isAdmin);
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
    }
}
