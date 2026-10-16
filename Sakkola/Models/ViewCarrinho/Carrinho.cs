namespace Sakkola.Models.ViewCarrinho
{
    public class Carrinho
    {
        public List<ItemCarrinho> Itens { get; set; } = new List<ItemCarrinho>();
        public decimal ValorTotal => Itens.Sum(i => i.Subtotal);
        public int TotalItens => Itens.Sum(i => i.Quantidade);
    }
}
