using Sakkola.Models.ViewModel;
using Sakkola.Models.ViewModel.Colaborador;
using Sakkola.Models.ViewModel.ColaboradorModel;
using Sakkola.Models.ViewPedidos;
using Sakkola.Models.ViewProdutos;

namespace Sakkola.Repository.Contract.ColaboradorContract
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
        Task<IEnumerable<Produtos>> ObterTodosProdutosAsync(string termoBusca = null, string categoria = null);
        Task DeletarProdutoAsync(int id);
        Task AtualizarProdutoAsync(ProdutoCadastrarViewModel model);
        Task<ProdutoCadastrarViewModel> ObterProdutoPorIdAsync(int id);
        Task<IEnumerable<UsuarioViewModel>> ObterTodosUsuariosAsync();
        Task AtualizarUsuarioAsync(UsuarioViewModel model);
        Task DeletarUsuarioAsync(int id);
    }
}
