namespace Sakkola.Models.ViewCarrinho
{
    public class ItemCarrinho
    {
        public int IdItem { get; set; }
        public int IdProduto { get; set; }
        public string NomeProduto { get; set; }
        public decimal PrecoUnitario { get; set; }
        public int Quantidade { get; set; }
        public string ImagemUrl { get; set; }
        public decimal Subtotal => PrecoUnitario * Quantidade;
    }
}
