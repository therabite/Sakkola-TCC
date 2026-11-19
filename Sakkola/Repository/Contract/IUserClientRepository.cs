using Sakkola.Models;
using Sakkola.Models.ViewCarrinho;
using Sakkola.Models.ViewModel;

namespace Sakkola.Repository.Interfaces
{
    public interface IUserClientRepository
    {
        Task<Carrinho?> ObterPorEmailAsync(string email)
        Task AtualizarClienteAsync(Carrinho cliente)
    }
}
