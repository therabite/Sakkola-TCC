using Sakkola.Models;
using Sakkola.Models.ViewModel;

namespace Sakkola.Services
{
    public interface IUsuarioServices
    {
        Task<bool> EmailJaCadastradoAsync(string email);
        Task CadastrarClienteAsync(RegisterClient model);
        Task<Usuario?> ValidarCredenciaisAsync(string email, string senha);
    }
}
