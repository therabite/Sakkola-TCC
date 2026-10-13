using Sakkola.Models.ViewProdutos;

namespace Sakkola.Repository.Interfaces
{
    public interface IProdutoRepository
    {
        Task<List<Produtos>> ObterTodosAsync();
        Task<List<Produtos>> ObterPorCategoriaAsync(string nomeCategoria);
        Task<Produtos> ObterPorIdAsync(int id);
        Task<List<Produtos>> ObterPorCategoriaExcetoIdAsync(int idCategoria, int idProdutoAtual, int limite = 4);
    }
}