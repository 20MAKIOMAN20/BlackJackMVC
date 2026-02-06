using Microsoft.AspNetCore.Mvc;
using BlackJackMVC.Services;

namespace BlackJackMVC.Controllers
{
	public class ControladorJuego : Controller
	{
		private readonly IServicioJuego _servicioJuego;

		public ControladorJuego(IServicioJuego servicioJuego)
		{
			_servicioJuego = servicioJuego;
		}

		public IActionResult Index()
		{
			var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
			var rol = HttpContext.Session.GetString("Rol");
			
			if (usuarioId == null || rol != "user")
			{
				return RedirectToAction("Login", "ControladorAutenticacion");
			}

			ViewBag.NombreUsuario = HttpContext.Session.GetString("NombreUsuario");
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Repartir(int apuesta)
		{
			var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
			if (usuarioId == null)
			{
				return Unauthorized();
			}

			try
			{
				var partida = await _servicioJuego.IniciarNuevaPartida(usuarioId.Value, apuesta);
				return Json(partida);
			}
			catch (Exception ex)
			{
				return BadRequest(new { mensaje = ex.Message });
			}
		}

		[HttpPost]
		public async Task<IActionResult> PedirCarta(int idPartida)
		{
			var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
			if (usuarioId == null)
			{
				return Unauthorized();
			}

			var partida = await _servicioJuego.JugadorPideCarta(idPartida);
			
			if (partida == null)
				return BadRequest("Partida no válida o ya terminada");
			
			return Json(partida);
		}

		[HttpPost]
		public async Task<IActionResult> Plantarse(int idPartida)
		{
			var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
			if (usuarioId == null)
			{
				return Unauthorized();
			}

			var partida = await _servicioJuego.JugadorSePlanta(idPartida);
			
			if (partida == null)
				return BadRequest("Partida no válida o ya terminada");
			
			return Json(partida);
		}
	}
}