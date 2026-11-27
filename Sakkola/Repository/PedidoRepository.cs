using MySqlConnector;
using Sakkola.Models;
using Sakkola.Models.ViewProdutos;
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

                sql += " ORDER BY id_pedido DESC;";

                using (var cmd = new MySqlCommand(sql, conexao))
                {
                    if (filtrar)
                    {
                        cmd.Parameters.AddWithValue("@Status", statusFiltro);
                    }

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        int ordinalData = reader.GetOrdinal("data_pedido");

                        while (await reader.ReadAsync())
                        {
                            DateTime dataPedido = DateTime.MinValue;
                            if (!await reader.IsDBNullAsync(ordinalData))
                            {
                                dataPedido = reader.GetDateTime(ordinalData);
                            }

                            lista.Add(new Pedido
                            {
                                IdPedido = reader.GetInt32("id_pedido"),
                                IdCliente = reader.GetInt32("id_user"),
                                ValorTotal = reader.GetDecimal("valor_total"),
                                DataPedido = dataPedido,
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

                string statusInicial = pedido.FormaPagamento != null && pedido.FormaPagamento.Contains("PIX") ? "Pago" : "Pendente";

                string sql = "INSERT INTO tbPedido (id_user, valor_total, forma_pagamento, status, data_pedido) VALUES (@clienteId, @total, @pagamento, @status, NOW()); SELECT LAST_INSERT_ID();";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@clienteId", pedido.IdCliente);
                    cmd.Parameters.AddWithValue("@total", pedido.ValorTotal);
                    cmd.Parameters.AddWithValue("@pagamento", pedido.FormaPagamento);
                    cmd.Parameters.AddWithValue("@status", statusInicial);

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
                string sqlPedido = "SELECT id_pedido, id_user, valor_total, data_pedido, status, forma_pagamento FROM tbpedido WHERE id_user = @IdUser ORDER BY data_pedido DESC;";

                using (var cmdPedido = new MySqlCommand(sqlPedido, conexao))
                {
                    cmdPedido.Parameters.AddWithValue("@IdUser", idUser);

                    using (var reader = await cmdPedido.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var dataPedido = reader.GetDateTime("data_pedido");
                            string status = reader.GetString("status");
                            string formaPagamento = reader.IsDBNull(reader.GetOrdinal("forma_pagamento")) ? "PIX" : reader.GetString("forma_pagamento");

                            if (status == "Pendente" && (DateTime.Now - dataPedido).TotalMinutes >= 5)
                            {
                                status = "Pago";
                            }

                            var pedido = new Pedido
                            {
                                IdPedido = reader.GetInt32("id_pedido"),
                                IdCliente = reader.GetInt32("id_user"),
                                ValorTotal = reader.GetDecimal("valor_total"),
                                DataPedido = dataPedido,
                                StatusPedido = status,
                                FormaPagamento = formaPagamento,
                                Itens = new List<ItemPedidoViewModel>()
                            };

                            lista.Add(pedido);
                        }
                    }
                }
                foreach (var pedido in lista)
                {
                    string sqlItens = @"
                        SELECT p.id_produto AS IdProduto, p.nome AS NomeProduto, p.descricao AS Descricao, 
                        ip.quantidade AS Quantidade, ip.preco_unidade AS PrecoUnitario
                        FROM tbItemPedido ip
                        INNER JOIN tbProduto p ON ip.fk_produto_id = p.id_produto
                        WHERE ip.fk_pedido_id = @IdPedido;";

                    using (var cmdItens = new MySqlCommand(sqlItens, conexao))
                    {
                        cmdItens.Parameters.AddWithValue("@IdPedido", pedido.IdPedido);

                        using (var readerItens = await cmdItens.ExecuteReaderAsync())
                        {
                            while (await readerItens.ReadAsync())
                            {
                                int idProduto = readerItens.GetInt32("IdProduto");

                                var item = new ItemPedidoViewModel
                                {
                                    NomeProduto = readerItens.GetString("NomeProduto"),
                                    Descricao = readerItens.IsDBNull(readerItens.GetOrdinal("Descricao")) ? "" : readerItens.GetString("Descricao"),
                                    ImagemUrl = idProduto,
                                    Quantidade = readerItens.GetInt32("Quantidade"),
                                    PrecoUnitario = readerItens.GetDecimal("PrecoUnitario")
                                };
                                pedido.Itens.Add(item);
                            }
                        }
                    }

                    if (pedido.Itens != null && pedido.Itens.Any())
                    {
                        pedido.ImagemUltimoItem = pedido.Itens.Last().ImagemUrl;
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
        public async Task CancelarPedidoAsync(int idPedido)
        {
            using (var conexao = new MySqlConnection(_connectionString))
            {
                await conexao.OpenAsync();
                string sql = "UPDATE tbpedido SET status = 'Cancelado' WHERE id_pedido = @IdPedido;";
                using (var cmd = new MySqlCommand(sql, conexao))
                {
                    cmd.Parameters.AddWithValue("@IdPedido", idPedido);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
