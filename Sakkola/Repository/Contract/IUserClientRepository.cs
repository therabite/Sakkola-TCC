using Sakkola.Models;
using Sakkola.Models.ViewCarrinho;
using Sakkola.Models.ViewModel;

namespace Sakkola.Repository.Interfaces
{
    public interface IUserClientRepository
    {
        Task<Client?> ObterPorEmailAsync(string email);
        Task AtualizarClienteAsync(Client cliente);
        Task CadastrarClienteAsync(RegisterClient client, Endereco endereco);
        Task<RegisterClient> ValidarUserAsync(string email, string senha);
        Task<bool> UserExistsAsync(string email);
    }
}
