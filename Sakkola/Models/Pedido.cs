namespace Sakkola.Models
{
    public class Pedido
    {
        public int IdPedido { get; set; }
        public int IdCliente { get; set; }
        public decimal ValorTotal { get; set; }
        public DateTime DataPedido { get; set; }
        public string StatusPedido { get; set; }
        public string FormaPagamento { get; set; }
        public List<ItemPedidoViewModel> Itens { get; set; } = new List<ItemPedidoViewModel>();
        public string ImagemUltimoItem => Itens?.LastOrDefault()?.ImagemUrl ?? "~/img/Logo.png";
    }
}
