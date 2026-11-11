using Microsoft.AspNetCore.Mvc;

namespace Sakkola.Controllers
{
    public class CepController : Controller
    {
        [HttpPost]
        public IActionResult CalcularFrete(string cepDestino)
        {
            if (string.IsNullOrEmpty(cepDestino))
            {
                return Json(new { sucesso = false, mensagem = "CEP inválido." });
            }

            cepDestino = cepDestino.Replace("-", "").Trim();

            string cepOrigem = "07769015";
            decimal valorFrete = 0.00m;

            if (cepDestino.StartsWith("07") || cepDestino.StartsWith("06"))
            {
                valorFrete = 12.00m;
            }
            else if (cepDestino.StartsWith("0"))
            {
                valorFrete = 18.00m;
            }
            else
            {
                valorFrete = 25.00m;
            }

            return Json(new
            {
                sucesso = true,
                frete = valorFrete,
                freteFormatado = "R$ " + valorFrete.ToString("N2")
            });
        }
    }
}