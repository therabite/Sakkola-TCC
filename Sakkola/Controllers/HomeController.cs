using Microsoft.AspNetCore.Mvc;
using Sakkola.Models;
using Sakkola.Repository.Interfaces;
using System.Diagnostics;

namespace Sakkola.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProdutoRepository _produtoRepository;
        public HomeController(IProdutoRepository produtoRepository)
        {
            _produtoRepository = produtoRepository;
        }
        public async Task<IActionResult> Index()
        {
            ViewData["TemaCategoria"] = "";

            var produtos = await _produtoRepository.ObterTodosAsync();
            return View(produtos);
        }
        [HttpGet]
        public async Task<IActionResult> Promocoes()
        {
            ViewData["TemaCategoria"] = "tema-promocao";
            ViewData["NomeCategoria"] = "Promoções";

            var produtosPromocao = await _produtoRepository.ObterPorCategoriaAsync("promocao");

            return View(produtosPromocao);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
