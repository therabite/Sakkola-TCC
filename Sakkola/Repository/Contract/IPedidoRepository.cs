using Sakkola.Models;

namespace Sakkola.Repository.Contract
{
    public interface IPedidoRepository
    {
        Task<int> CadastrarPedidoAsync(Pedido pedido);
        Task<IEnumerable<Pedido>> ObterPedidosPorStatusOuTodosAsync(string status);
        Task<IEnumerable<Pedido>> ObterPedidosPorUsuarioAsync(int idUser);
        Task AtualizarStatusPedidoAsync(int idPedido, string novoStatus);
    }
}
