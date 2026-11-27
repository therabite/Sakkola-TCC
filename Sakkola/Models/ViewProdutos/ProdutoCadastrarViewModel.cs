namespace Sakkola.Models.ViewProdutos
{
    public class ProdutoCadastrarViewModel
    {
        public int IdProduto { get; set; }  
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public string Preco { get; set; }
        public int Estoque { get; set; }
        public string Categoria { get; set; }
        public IFormFile Imagem { get; set; }
    }
}
