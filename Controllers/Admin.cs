using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlackJackMVC.Services;
using BlackJackMVC.Models;
using BlackJackMVC.Data;
using System.Linq;

namespace BlackJackMVC.Controllers
{
	public class Admin : Controller
	{
		private readonly IServicioAdmin _servicioAdmin;
		private readonly ContextoBaseDatos _contexto;

		public Admin(IServicioAdmin servicioAdmin, ContextoBaseDatos contexto)
		{
			_servicioAdmin = servicioAdmin;
			_contexto = contexto;
		}

		private bool EsAdmin()
		{
			var rol = HttpContext.Session.GetString("Rol");
			return rol == "admin";
		}

		public async Task<IActionResult> Index()
		{
			if (!EsAdmin())
			{
				return RedirectToAction("Login", "ControladorAutenticacion");
			}

			ViewBag.NombreUsuario = HttpContext.Session.GetString("NombreUsuario");

			var estadisticas = await _servicioAdmin.ObtenerEstadisticasGlobales();
			var ultimosUsuarios = await _servicioAdmin.ObtenerTodosUsuarios();
			
			ultimosUsuarios = ultimosUsuarios.Take(5).ToList();

			var dashboard = new DashboardAdmin
			{
				Estadisticas = estadisticas,
				UltimosUsuarios = ultimosUsuarios
			};

			return View(dashboard);
		}

		public async Task<IActionResult> Usuarios()
		{
			if (!EsAdmin())
			{
				return RedirectToAction("Login", "ControladorAutenticacion");
			}

			ViewBag.NombreUsuario = HttpContext.Session.GetString("NombreUsuario");

			var usuarios = await _servicioAdmin.ObtenerTodosUsuarios();
			return View(usuarios);
		}
		public async Task<IActionResult> DetalleUsuario(int id)
		{
			if (!EsAdmin())
			{
				return RedirectToAction("Login", "ControladorAutenticacion");
			}

			ViewBag.NombreUsuario = HttpContext.Session.GetString("NombreUsuario");

			var usuario = await _servicioAdmin.ObtenerUsuarioPorId(id);
			if (usuario == null)
			{
				return RedirectToAction("Usuarios");
			}

			return View(usuario);
		}

		[HttpPost]
		public async Task<IActionResult> DarFichas(int usuarioId, int cantidad)
		{
			if (!EsAdmin())
			{
				return Unauthorized();
			}

			var resultado = await _servicioAdmin.DarFichas(usuarioId, cantidad);
			return Json(new { exitoso = resultado, mensaje = resultado ? "Fichas agregadas correctamente" : "Error al agregar fichas" });
		}

		[HttpPost]
		public async Task<IActionResult> QuitarFichas(int usuarioId, int cantidad)
		{
			if (!EsAdmin())
			{
				return Unauthorized();
			}

			var resultado = await _servicioAdmin.QuitarFichas(usuarioId, cantidad);
			return Json(new { exitoso = resultado, mensaje = resultado ? "Fichas quitadas correctamente" : "Error al quitar fichas" });
		}

		[HttpPost]
		public async Task<IActionResult> EliminarUsuario(int usuarioId)
		{
			if (!EsAdmin())
			{
				return Unauthorized();
			}

			var resultado = await _servicioAdmin.EliminarUsuario(usuarioId);
			return Json(new { exitoso = resultado, mensaje = resultado ? "Usuario eliminado correctamente" : "Error al eliminar usuario" });
		}
		
		public async Task<IActionResult> UsuariosEliminados()
		{
			if (!EsAdmin())
			{
				return RedirectToAction("Login", "ControladorAutenticacion");
			}

			ViewBag.NombreUsuario = HttpContext.Session.GetString("NombreUsuario");

			var usuarios = await _servicioAdmin.ObtenerUsuariosEliminados();
			return View(usuarios);
		}

		[HttpPost]
		public async Task<IActionResult> RestaurarUsuario(int usuarioId)
		{
			if (!EsAdmin())
			{
				return Unauthorized();
			}

			var resultado = await _servicioAdmin.RestaurarUsuario(usuarioId);
			return Json(new { exitoso = resultado, mensaje = resultado ? "Usuario restaurado correctamente" : "Error al restaurar usuario" });
		}
		[HttpPost]
		public async Task<IActionResult> EliminarUsuarioPermanente(int usuarioId)
		{
			if (!EsAdmin())
			{
				return Unauthorized();
			}

			var resultado = await _servicioAdmin.EliminarUsuarioPermanente(usuarioId);
			return Json(new { exitoso = resultado, mensaje = resultado ? "Usuario eliminado permanentemente" : "Error al eliminar usuario" });
		}
		
		public async Task<IActionResult> Tienda()
		{
			if (!EsAdmin())
			{
				return RedirectToAction("Login", "ControladorAutenticacion");
			}

			ViewBag.NombreUsuario = HttpContext.Session.GetString("NombreUsuario");

			var articulos = await _servicioAdmin.ObtenerTodosArticulos();
			
			var articulosConStats = new List<ArticuloConEstadisticas>();

			foreach (var a in articulos)
			{
				var totalVendidos = await _contexto.ComprasUsuarios.CountAsync(c => c.ArticuloId == a.Id);
				
				articulosConStats.Add(new ArticuloConEstadisticas
				{
					Id = a.Id,
					Nombre = a.Nombre,
					Descripcion = a.Descripcion,
					PrecioFichas = a.PrecioFichas,
					TipoArticulo = a.TipoArticulo,
					ImagenUrl = a.ImagenUrl,
					Disponible = a.Disponible,
					TotalVendidos = totalVendidos,
					FichasGeneradas = totalVendidos * a.PrecioFichas
				});
			}
			
			return View(articulosConStats);
		}
		
		public IActionResult CrearArticulo()
		{
			if (!EsAdmin())
			{
				return RedirectToAction("Login", "ControladorAutenticacion");
			}

			ViewBag.NombreUsuario = HttpContext.Session.GetString("NombreUsuario");
			return View();
		}
		
		[HttpPost]
		public async Task<IActionResult> CrearArticulo(ArticuloTienda articulo)
		{
			if (!EsAdmin())
			{
				return Unauthorized();
			}

			articulo.Disponible = true; // Por defecto activo
			var resultado = await _servicioAdmin.CrearArticulo(articulo);
			
			if (resultado)
			{
				return RedirectToAction("Tienda");
			}

			ModelState.AddModelError("", "Error al crear el artículo");
			return View(articulo);
		}
		
		public async Task<IActionResult> EditarArticulo(int id)
		{
			if (!EsAdmin())
			{
				return RedirectToAction("Login", "ControladorAutenticacion");
			}

			ViewBag.NombreUsuario = HttpContext.Session.GetString("NombreUsuario");

			var articulo = await _servicioAdmin.ObtenerArticuloPorId(id);
			if (articulo == null)
			{
				return RedirectToAction("Tienda");
			}

			return View(articulo);
		}
		
		[HttpPost]
		public async Task<IActionResult> EditarArticulo(ArticuloTienda articulo)
		{
			if (!EsAdmin())
			{
				return Unauthorized();
			}

			var resultado = await _servicioAdmin.EditarArticulo(articulo);
			
			if (resultado)
			{
				return RedirectToAction("Tienda");
			}

			ModelState.AddModelError("", "Error al editar el artículo");
			return View(articulo);
		}

		[HttpPost]
		public async Task<IActionResult> CambiarEstadoArticulo(int articuloId, bool disponible)
		{
			if (!EsAdmin())
			{
				return Unauthorized();
			}

			var resultado = await _servicioAdmin.CambiarEstadoArticulo(articuloId, disponible);
			return Json(new { exitoso = resultado, mensaje = resultado ? "Estado actualizado" : "Error al actualizar" });
		}
	}
}