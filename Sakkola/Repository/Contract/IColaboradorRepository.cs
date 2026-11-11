using Sakkola.Models;
using Sakkola.Models.ViewModel;

namespace Sakkola.Repository.Interfaces
{
    public interface IColaboradorRepository
    {
        Task<List<Pedido>> ObterPedidosPendentesAsync();
        Task AtualizarStatusPedidoAsync(int idPedido, string novoStatus);
        Task CadastrarPedidoAsync(Pedido pedido);
        Task<LoginColaborador> ValidarColaboradorAsync(string email, string senha);
        Task CadastrarFuncionarioAsync(RegisterColaborador colaborador);
    }
}
