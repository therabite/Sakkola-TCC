using Sakkola.Models;
using Sakkola.Models.ViewModel;
using Sakkola.Models.ViewProdutos;

namespace Sakkola.Repository.Interfaces
{
    public interface IColaboradorRepository
    {
        Task<List<Pedido>> ObterPedidosPendentesAsync();
        Task AtualizarStatusPedidoAsync(int idPedido, string novoStatus);
        Task CadastrarPedidoAsync(Pedido pedido);
        Task<Colaborador> ValidarColaboradorAsync(string email, string senha);
        Task CadastrarFuncionarioAsync(RegisterColaborador colaborador);
        Task<int> InserirProdutoAsync(ProdutoCadastrarViewModel model);
        Task<IEnumerable<string>> ObterCategoriasAsync();
        Task<IEnumerable<Produtos>> ObterTodosProdutosAsync();
        Task DeletarProdutoAsync(int id);
        Task AtualizarProdutoAsync(int id, ProdutoCadastrarViewModel model);
        Task<ProdutoCadastrarViewModel> ObterProdutoPorIdAsync(int id);
    }
}
