namespace Sakkola.Models.ViewProdutos
{
    public class Produtos : Categoria
    {
        public int id { get; set; }
        public string nome { get; set; }
        public string descricao { get; set; }
        public decimal preco { get; set; }
        public int estoque { get; set; }
        public int id_categoria { get; set; }
        public Categoria Categoria { get; set; }
    }
}
