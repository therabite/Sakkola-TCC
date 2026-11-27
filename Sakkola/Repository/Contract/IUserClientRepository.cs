using Sakkola.Models;
using Sakkola.Models.ViewModel;

namespace Sakkola.Repository.Interfaces
{
    public interface IUserClientRepository
    {
        Task<Sakkola.Models.ViewModel.Client?> ObterPorEmailAsync(string email);
        Task AtualizarClienteAsync(Sakkola.Models.ViewModel.Client cliente);
        Task CadastrarClienteAsync(RegisterClient client, Endereco endereco);
        Task<RegisterClient> ValidarUserAsync(string email, string senha);
        Task<bool> UserExistsAsync(string email);
    }
}
