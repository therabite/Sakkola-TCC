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

        public async Task<IActionResult> Index()
        {
            var produtos = await _produtoRepository.ObterTodosAsync();
            return View(produtos);
        }

        public async Task<IActionResult> Categoria(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index", "Home");
            }

            string categoriaSlug = id.ToLower().Trim();
            if (categoriaSlug == "promoção") categoriaSlug = "promocao";

            ViewData["TemaCategoria"] = categoriaSlug switch
            {
                "acougue" => "tema-acougue",
                "hortifruti" => "tema-hortifruti",
                "padaria" => "tema-padaria",
                "higiene" => "tema-higiene",
                "limpeza" => "tema-limpeza",
                "frios" => "tema-frios",
                "mercearia" => "tema-mercearia",
                "promocao" => "tema-promocao",
                _ => ""
            };

            ViewData["NomeCategoria"] = categoriaSlug switch
            {
                "acougue" => "Açougue",
                "hortifruti" => "Hortifrúti",
                "padaria" => "Padaria",
                "higiene" => "Higiene",
                "limpeza" => "Limpeza e Utilidades",
                "frios" => "Frios e Laticínios",
                "mercearia" => "Mercearia",
                "promocao" => "Promoções",
                _ => id
            };

            ViewData["IdCategoria"] = categoriaSlug;

            var produtos = await _produtoRepository.ObterPorCategoriaAsync(categoriaSlug);

            return View(produtos);
        }
    }
}