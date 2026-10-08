using MySqlConnector;
using Sakkola.Models;
using Sakkola.Models.ViewModel;
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

                using(var cmd = new MySqlCommand(sql, conn))
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
    }
}
