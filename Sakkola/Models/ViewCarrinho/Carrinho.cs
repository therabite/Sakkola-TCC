namespace Sakkola.Models.ViewCarrinho
{
    public class Carrinho
    {
        public List<ItemCarrinho> Itens { get; set; } = new List<ItemCarrinho>();
        public decimal ValorTotal => Itens.Sum(i => i.Subtotal);
        public int TotalItens => Itens.Sum(i => i.Quantidade);

        public string NomeCliente { get; set; }
        public string Telefone { get; set; }

        public string Cep { get; set; }
        public string Logradouro { get; set; }
        public string Numero { get; set; }
        public string Bairro { get; set; }
        public string Cidade { get; set; }
        public string Estado { get; set; }
    }
}
