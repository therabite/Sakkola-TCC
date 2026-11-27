using MySqlConnector;
using Sakkola.Models;
using Sakkola.Models.ViewCarrinho;
using Sakkola.Models.ViewModel.Client;
using Sakkola.Repository.Interfaces;
using System.Net;
using System.Text.RegularExpressions;

namespace Sakkola.Repository
{
    public class UserClientRepository : IUserClientRepository
    {
        private readonly string _connectionString;

        public UserClientRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("conexaoMySQL")
            ?? throw new ArgumentNullException(nameof(configuration));
        }
        public async Task CadastrarClienteAsync(RegisterClient client, Endereco endereco)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string sql = @"
                    INSERT INTO tbAddress (CEP) VALUES (@cep);
                    INSERT INTO tbUser (nome, data_nasc, telefone, CPF, email, senha) 
                    VALUES (@nome, @data_nasc, @telefone, @CPF, @email, @senha);
                    INSERT INTO tbClient (id_client) VALUES (LAST_INSERT_ID());";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    string cepLimpo = Regex.Replace(endereco.Cep ?? "", @"\D", "");
                    string cpfLimpo = Regex.Replace(client.cpf ?? "", @"\D", "");
                    string telefoneLimpo = Regex.Replace(client.telefone ?? "", @"\D", "");

                    cmd.Parameters.AddWithValue("@cep", cepLimpo);
                    cmd.Parameters.AddWithValue("@nome", client.nome);
                    cmd.Parameters.AddWithValue("@data_nasc", client.data_nasc);
                    cmd.Parameters.AddWithValue("@telefone", telefoneLimpo);
                    cmd.Parameters.AddWithValue("@CPF", cpfLimpo);
                    cmd.Parameters.AddWithValue("@email", client.email);
                    cmd.Parameters.AddWithValue("@senha", client.senha);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
        public async Task<bool> UserExistsAsync(string email)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string sql = "SELECT COUNT(1) FROM tbuser WHERE email = @email;";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@email", email);

                    var result = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    return result > 0;
                }
            }
        }
        public async Task<RegisterClient> ValidarUserAsync(string email, string senha)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string sql = @"SELECT Id_user, nome, email FROM tbUser WHERE email = @email AND senha = @senha LIMIT 1;";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@senha", senha);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new RegisterClient
                            {
                                Id_user = reader.GetInt32("Id_user"),
                                nome = reader.GetString("nome"),
                                email = reader.GetString("email")
                            };
                        }
                    }
                }
            }
            return null;
        }
        public async Task<Client?> ObterPorEmailAsync(string email)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            string sql = @"SELECT u.id_user, u.nome AS NomeCliente, 
                  u.email AS Email, 
                  u.telefone AS Telefone, 
                  a.CEP AS Cep, 
                  a.Logradouro AS Logradouro, 
                  a.Numero AS Numero, 
                  a.Bairro AS Bairro, 
                  a.Cidade AS Cidade, 
                  a.Estado AS Estado 
           FROM tbUser u
           LEFT JOIN tbAddress a ON u.id_user = a.id_address
           WHERE u.email = @Email;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Email", email);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Client
                {
                    Id_client = reader["id_user"] != DBNull.Value ? Convert.ToInt32(reader["id_user"]) : 0,
                    Nome = reader["NomeCliente"] != DBNull.Value ? reader["NomeCliente"].ToString() : string.Empty,
                    Email = reader["Email"] != DBNull.Value ? reader["Email"].ToString() : string.Empty,
                    Telefone = reader["Telefone"] != DBNull.Value ? reader["Telefone"].ToString() : string.Empty,
                    // Inicializa o objeto EnderecoEntrega corretamente para evitar erros de contexto
                    EnderecoEntrega = new Endereco
                    {
                        Cep = reader["Cep"] != DBNull.Value ? reader["Cep"].ToString() : string.Empty,
                        Logradouro = reader["Logradouro"] != DBNull.Value ? reader["Logradouro"].ToString() : string.Empty,
                        Numero = reader["Numero"] != DBNull.Value ? reader["Numero"].ToString() : string.Empty,
                        Bairro = reader["Bairro"] != DBNull.Value ? reader["Bairro"].ToString() : string.Empty,
                        Cidade = reader["Cidade"] != DBNull.Value ? reader["Cidade"].ToString() : string.Empty,
                        Estado = reader["Estado"] != DBNull.Value ? reader["Estado"].ToString() : string.Empty
                    }
                };
            }

            return null;
        }
        public async Task AtualizarClienteAsync(Client cliente)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            int idUsuario = 0;
            string sqlFindUser = "SELECT id_user FROM tbUser WHERE email = @Email;";
            using (var cmdFind = new MySqlCommand(sqlFindUser, conn))
            {
                cmdFind.Parameters.AddWithValue("@Email", cliente.Email);
                var result = await cmdFind.ExecuteScalarAsync();
                if (result != null)
                {
                    idUsuario = Convert.ToInt32(result);
                }
            }

            if (idUsuario == 0) return;

            using (var tran = await conn.BeginTransactionAsync())
            {
                try
                {
                    // 1. Atualizar Nome e Telefone na tabela tbUser
                    string sqlUser = "UPDATE tbUser SET nome = @nome, telefone = @telefone WHERE id_user = @idUsuario;";
                    using (var cmdUser = new MySqlCommand(sqlUser, conn, tran))
                    {
                        cmdUser.Parameters.AddWithValue("@nome", cliente.Nome ?? (object)DBNull.Value);
                        cmdUser.Parameters.AddWithValue("@telefone", cliente.Telefone ?? (object)DBNull.Value);
                        cmdUser.Parameters.AddWithValue("@idUsuario", idUsuario); // idUsuario é int, sem operador ??
                        await cmdUser.ExecuteNonQueryAsync();
                    }

                    // 2. Verificar se o endereço já existe para este usuário em tbAddress
                    string sqlCheckEnd = "SELECT COUNT(1) FROM tbAddress WHERE id_address = @idUsuario;";
                    long enderecoExiste = 0;
                    using (var cmdCheck = new MySqlCommand(sqlCheckEnd, conn, tran))
                    {
                        cmdCheck.Parameters.AddWithValue("@idUsuario", idUsuario);
                        enderecoExiste = Convert.ToInt64(await cmdCheck.ExecuteScalarAsync());
                    }

                    // Garante que EnderecoEntrega não seja nulo para evitar exceções
                    var end = cliente.EnderecoEntrega ?? new Endereco();

                    if (enderecoExiste > 0)
                    {
                        // Atualiza o endereço existente
                        string sqlUpdateEnd = @"
                UPDATE tbAddress 
                SET CEP = @cep, Logradouro = @logradouro, Numero = @numero, 
                    Bairro = @bairro, Cidade = @cidade, Estado = @estado 
                WHERE id_address = @idUsuario;";

                        using (var cmdUpEnd = new MySqlCommand(sqlUpdateEnd, conn, tran))
                        {
                            cmdUpEnd.Parameters.AddWithValue("@cep", end.Cep ?? (object)DBNull.Value);
                            cmdUpEnd.Parameters.AddWithValue("@logradouro", end.Logradouro ?? (object)DBNull.Value);
                            cmdUpEnd.Parameters.AddWithValue("@numero", end.Numero ?? (object)DBNull.Value);
                            cmdUpEnd.Parameters.AddWithValue("@bairro", end.Bairro ?? (object)DBNull.Value);
                            cmdUpEnd.Parameters.AddWithValue("@cidade", end.Cidade ?? (object)DBNull.Value);
                            cmdUpEnd.Parameters.AddWithValue("@estado", end.Estado ?? (object)DBNull.Value);
                            cmdUpEnd.Parameters.AddWithValue("@idUsuario", idUsuario);
                            await cmdUpEnd.ExecuteNonQueryAsync();
                        }
                    }
                    else
                    {
                        // Insere um novo endereço caso não exista
                        string sqlInsEnd = @"
                INSERT INTO tbAddress (id_address, CEP, Logradouro, Numero, Bairro, Cidade, Estado) 
                VALUES (@idUsuario, @cep, @logradouro, @numero, @bairro, @cidade, @estado);";

                        using (var cmdInsEnd = new MySqlCommand(sqlInsEnd, conn, tran))
                        {
                            cmdInsEnd.Parameters.AddWithValue("@idUsuario", idUsuario);
                            cmdInsEnd.Parameters.AddWithValue("@cep", end.Cep ?? (object)DBNull.Value);
                            cmdInsEnd.Parameters.AddWithValue("@logradouro", end.Logradouro ?? (object)DBNull.Value);
                            cmdInsEnd.Parameters.AddWithValue("@numero", end.Numero ?? (object)DBNull.Value);
                            cmdInsEnd.Parameters.AddWithValue("@bairro", end.Bairro ?? (object)DBNull.Value);
                            cmdInsEnd.Parameters.AddWithValue("@cidade", end.Cidade ?? (object)DBNull.Value);
                            cmdInsEnd.Parameters.AddWithValue("@estado", end.Estado ?? (object)DBNull.Value);
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
}