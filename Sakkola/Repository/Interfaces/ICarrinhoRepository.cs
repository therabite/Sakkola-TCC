using Sakkola.Models.ViewCarrinho;
namespace Sakkola.Repository.Interfaces
{
    public interface ICarrinhoRepository
    {
        Task<Carrinho> ObterCarrinhoPorUsuarioAsync(string userId);
        Task AdicionarItemAsync(string userId, int idProduto, int quantidade);
        Task AtualizarQuantidadeAsync(string userId, int idItem, int quantidade);
        Task RemoverItemAsync(string userId, int idItem);
    }
}
