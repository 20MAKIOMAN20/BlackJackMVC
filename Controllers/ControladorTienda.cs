using Microsoft.AspNetCore.Mvc;
using BlackJackMVC.Services;

namespace BlackJackMVC.Controllers
{
	public class ControladorTienda : Controller
	{
		private readonly IServicioTienda _servicioTienda;

		public ControladorTienda(IServicioTienda servicioTienda)
		{
			_servicioTienda = servicioTienda;
		}

		public async Task<IActionResult> Index()
		{
			var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
			var rol = HttpContext.Session.GetString("Rol");
			
			if (usuarioId == null || rol != "user")
			{
				return RedirectToAction("Login", "ControladorAutenticacion");
			}

			ViewBag.NombreUsuario = HttpContext.Session.GetString("NombreUsuario");
			var datosTienda = await _servicioTienda.ObtenerDatosTienda(usuarioId.Value);
			return View(datosTienda);
		}

		[HttpPost]
		public async Task<IActionResult> Comprar([FromBody] CompraRequest request)
		{
			var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
			if (usuarioId == null)
			{
				return Unauthorized();
			}

			if (usuarioId.Value != request.UsuarioId)
			{
				return Forbid();
			}

			var resultado = await _servicioTienda.ComprarArticulo(request.UsuarioId, request.ArticuloId);
			return Json(resultado);
		}

		[HttpGet]
		public async Task<IActionResult> ObtenerFichas()
		{
			var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
			if (usuarioId == null)
			{
				return Unauthorized();
			}

			var fichas = await _servicioTienda.ObtenerFichasUsuario(usuarioId.Value);
			return Json(new { fichas });
		}
		
		public class CompraRequest
		{
			public int UsuarioId { get; set; }
			public int ArticuloId { get; set; }
		}
	}
}