using Microsoft.AspNetCore.Mvc;
using BlackJackMVC.Models;
using BlackJackMVC.Services;

namespace BlackJackMVC.Controllers
{
	public class ControladorAutenticacion : Controller
	{
		private readonly IServicioAutenticacion _servicioAuth;

		public ControladorAutenticacion(IServicioAutenticacion servicioAuth)
		{
			_servicioAuth = servicioAuth;
		}

		[HttpGet]
		public IActionResult Login()
		{
			if (HttpContext.Session.GetInt32("UsuarioId") != null)
			{
				return RedirectToAction("Index", "ControladorJuego");
			}

			return View();
		}
		
		// Iniciar
		[HttpPost]
		public async Task<IActionResult> Login(ModeloLogin modelo)
		{
			if (!ModelState.IsValid)
			{
				return View(modelo);
			}

			var resultado = await _servicioAuth.IniciarSesion(modelo);

			if (resultado.Exitoso && resultado.Usuario != null)
			{
				HttpContext.Session.SetInt32("UsuarioId", resultado.Usuario.Id);
				HttpContext.Session.SetString("NombreUsuario", resultado.Usuario.Nombre);
				HttpContext.Session.SetString("Rol", resultado.Usuario.Rol);
				
				if (resultado.Usuario.Rol == "admin")
				{
					return RedirectToAction("Index", "Admin");
				}
				else
				{
					return RedirectToAction("Index", "ControladorJuego");
				}
			}
			
			ModelState.AddModelError("", resultado.Mensaje);
			return View(modelo);
		}


		[HttpGet]
		public IActionResult Registro()
		{
			if (HttpContext.Session.GetInt32("UsuarioId") != null)
			{
				return RedirectToAction("Index", "ControladorJuego");
			}

			return View();
		}

		// Crear
		[HttpPost]
		public async Task<IActionResult> Registro(ModeloRegistro modelo)
		{
			if (!ModelState.IsValid)
			{
				return View(modelo);
			}

			var resultado = await _servicioAuth.RegistrarUsuario(modelo);

			if (resultado.Exitoso && resultado.Usuario != null)
			{
				HttpContext.Session.SetInt32("UsuarioId", resultado.Usuario.Id);
				HttpContext.Session.SetString("NombreUsuario", resultado.Usuario.Nombre);
				HttpContext.Session.SetString("Rol", resultado.Usuario.Rol);

				return RedirectToAction("Index", "ControladorJuego");
			}

			ModelState.AddModelError("", resultado.Mensaje);
			return View(modelo);
		}

		// Cerrar
		public IActionResult CerrarSesion()
		{
			HttpContext.Session.Clear();
			return RedirectToAction("Login");
		}
	}
}