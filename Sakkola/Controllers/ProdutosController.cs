using Microsoft.AspNetCore.Mvc;
using Sakkola.Repository.Interfaces;

namespace Sakkola.Controllers
{
    public class ProdutosController : Controller
    {
        private readonly IProdutoRepository _produtoRepository;

        public ProdutosController(IProdutoRepository produtoRepository)
        {
            _produtoRepository = produtoRepository;
        }

        // Listagem geral / Busca
        public async Task<IActionResult> Index()
        {
            var produtos = await _produtoRepository.ObterTodosAsync();
            return View(produtos);
        }

        // Filtro por Categoria (recebe string da URL)
        public async Task<IActionResult> Categoria(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["TemaCategoria"] = id.ToLower() switch
            {
                "acougue" => "tema-acougue",
                "hortifruti" => "tema-hortifruti",
                "padaria" => "tema-padaria",
                "higiene" => "tema-higiene",
                "limpeza" => "tema-limpeza",
                "frios" => "tema-frios",
                "mercearia" => "tema-mercearia",
                _ => ""
            };

            ViewData["NomeCategoria"] = id.ToLower() switch
            {
                "acougue" => "Açougue",
                "hortifruti" => "Hortifrúti",
                "padaria" => "Padaria",
                "higiene" => "Higiene",
                "limpeza" => "Limpeza e Utilidades",
                "frios" => "Frios e Laticínios",
                "mercearia" => "Mercearia",
                _ => id
            };

            ViewData["IdCategoria"] = id.ToLower();

            // Usa corretamente o método que aceita string
            var produtos = await _produtoRepository.ObterPorCategoriaAsync(id);

            return View(produtos);
        }

        // Tela de Detalhes do Produto (recebe inteiro do ID)
        public async Task<IActionResult> Detalhes(int id)
        {
            var produto = await _produtoRepository.ObterPorIdAsync(id);
            if (produto == null)
            {
                return NotFound();
            }

            string nomeCatSlug = produto.Categoria?.categoria?.ToLower() ?? "acougue";

            if (nomeCatSlug.Contains("açougue") || nomeCatSlug.Contains("acougue")) nomeCatSlug = "acougue";
            else if (nomeCatSlug.Contains("frios")) nomeCatSlug = "frios";
            else if (nomeCatSlug.Contains("hortifrúti") || nomeCatSlug.Contains("hortifruti")) nomeCatSlug = "hortifruti";
            else if (nomeCatSlug.Contains("padaria")) nomeCatSlug = "padaria";
            else if (nomeCatSlug.Contains("higiene")) nomeCatSlug = "higiene";
            else if (nomeCatSlug.Contains("limpeza")) nomeCatSlug = "limpeza";
            else if (nomeCatSlug.Contains("mercearia")) nomeCatSlug = "mercearia";

            ViewData["TemaCategoria"] = $"tema-{nomeCatSlug}";
            ViewData["NomeCategoria"] = produto.Categoria?.categoria ?? "Produtos";
            ViewData["IdCategoria"] = nomeCatSlug;

            // Busca as sugestões utilizando os IDs inteiros corretos
            var sugestoes = await _produtoRepository.ObterPorCategoriaExcetoIdAsync(produto.id_categoria, produto.id, 4);
            ViewBag.Sugestoes = sugestoes;

            return View(produto);
        }
    }
}