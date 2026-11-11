namespace Sakkola.Models
{
    public class Pedido
    {
        public int IdPedido { get; set; }
        public int IdCliente { get; set; }
        public string NomeCliente { get; set; }
        public decimal ValorTotal { get; set; }
        public DateTime DataPedido { get; set; }
        public string StatusPedido { get; set; }
        public string FormaPagamento { get; set; }
    }
}
