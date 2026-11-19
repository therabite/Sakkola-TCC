using MySqlConnector;
using Sakkola.Models;
using Sakkola.Models.ViewCarrinho;
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

        public async Task<Carrinho?> ObterPorEmailAsync(string email)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            // Ajuste os nomes das colunas e tabelas conforme o seu banco de dados
            string sql = @"SELECT nome AS NomeCliente, email AS Email, telefone AS Telefone, 
                          cep AS Cep, logradouro AS Logradouro, numero AS Numero, 
                          bairro AS Bairro, cidade AS Cidade, estado AS Estado 
                   FROM tbCliente 
                   WHERE email = @Email;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Email", email);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Carrinho
                {
                    NomeCliente = reader["NomeCliente"].ToString(),
                    Email = reader["Email"].ToString(),
                    Telefone = reader["Telefone"].ToString(),
                    Cep = reader["Cep"].ToString(),
                    Logradouro = reader["Logradouro"].ToString(),
                    Numero = reader["Numero"].ToString(),
                    Bairro = reader["Bairro"].ToString(),
                    Cidade = reader["Cidade"].ToString(),
                    Estado = reader["Estado"].ToString()
                };
            }

            return null;
        }

        public async Task AtualizarClienteAsync(Carrinho cliente)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            string sql = @"UPDATE tbCliente 
                   SET nome = @Nome, 
                       telefone = @Telefone, 
                       cep = @Cep, 
                       logradouro = @Logradouro, 
                       numero = @Numero, 
                       bairro = @Bairro, 
                       cidade = @Cidade, 
                       estado = @Estado
                   WHERE email = @Email;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Nome", cliente.NomeCliente);
            cmd.Parameters.AddWithValue("@Telefone", cliente.Telefone);
            cmd.Parameters.AddWithValue("@Cep", cliente.Cep);
            cmd.Parameters.AddWithValue("@Logradouro", cliente.Logradouro);
            cmd.Parameters.AddWithValue("@Numero", cliente.Numero);
            cmd.Parameters.AddWithValue("@Bairro", cliente.Bairro ?? "");
            cmd.Parameters.AddWithValue("@Cidade", cliente.Cidade);
            cmd.Parameters.AddWithValue("@Estado", cliente.Estado ?? "");
            cmd.Parameters.AddWithValue("@Email", cliente.Email);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
