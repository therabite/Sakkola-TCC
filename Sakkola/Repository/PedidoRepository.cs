using MySqlConnector;
using Sakkola.Models;
using Sakkola.Repository.Contract;

namespace Sakkola.Repository
{
    public class PedidoRepository : IPedidoRepository
    {
        private readonly string _connectionString;
        public PedidoRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("conexaoMySQL");
        }
        public async Task<IEnumerable<Pedido>> ObterPedidosPorStatusOuTodosAsync(string statusFiltro)
        {
            var lista = new List<Pedido>();

            using (var conexao = new MySqlConnection(_connectionString))
            {
                await conexao.OpenAsync();

                string sql = "SELECT * FROM tbpedido";
                bool filtrar = !string.IsNullOrEmpty(statusFiltro);

                if (filtrar)
                {
                    sql += " WHERE status = @Status";
                }

                using (var cmd = new MySqlCommand(sql, conexao))
                {
                    if (filtrar)
                    {
                        cmd.Parameters.AddWithValue("@Status", statusFiltro);
                    }

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            lista.Add(new Pedido
                            {
                                IdPedido = reader.GetInt32("id_pedido"),
                                IdCliente = reader.GetInt32("id_user"),
                                ValorTotal = reader.GetDecimal("valor_total"),
                                StatusPedido = reader.GetString("status")
                            });
                        }
                    }
                }
            }
            return lista;
        }
        public async Task<int> CadastrarPedidoAsync(Pedido pedido)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string sql = "INSERT INTO tbPedido (id_user, valor_total, forma_pagamento, status) VALUES (@clienteId, @total, @pagamento, 'Pendente'); SELECT LAST_INSERT_ID();";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@clienteId", pedido.IdCliente);
                    cmd.Parameters.AddWithValue("@total", pedido.ValorTotal);
                    cmd.Parameters.AddWithValue("@pagamento", pedido.FormaPagamento);

                    return Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }
            }
        }
        public async Task<IEnumerable<Pedido>> ObterPedidosPorUsuarioAsync(int idUser)
        {
            var lista = new List<Pedido>();

            using (var conexao = new MySqlConnection(_connectionString))
            {
                await conexao.OpenAsync();

                string sql = "SELECT * FROM tbpedido WHERE id_user = @IdUser ORDER BY data_pedido DESC;";

                using (var cmd = new MySqlCommand(sql, conexao))
                {
                    cmd.Parameters.AddWithValue("@IdUser", idUser);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            lista.Add(new Pedido
                            {
                                IdPedido = reader.GetInt32("id_pedido"),
                                IdCliente = reader.GetInt32("id_user"),
                                ValorTotal = reader.GetDecimal("valor_total"),
                                DataPedido = reader.GetDateTime("data_pedido"),
                                StatusPedido = reader.GetString("status")
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public async Task AtualizarStatusPedidoAsync(int idPedido, string novoStatus)
        {
            using (var conexao = new MySqlConnection(_connectionString))
            {
                await conexao.OpenAsync();

                string sql = "UPDATE tbpedido SET status = @NovoStatus WHERE id_pedido = @IdPedido;";

                using (var cmd = new MySqlCommand(sql, conexao))
                {
                    cmd.Parameters.AddWithValue("@NovoStatus", novoStatus);
                    cmd.Parameters.AddWithValue("@IdPedido", idPedido);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
