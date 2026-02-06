using Microsoft.EntityFrameworkCore;
using BlackJackMVC.Models;
using BlackJackMVC.Data;

namespace BlackJackMVC.Services
{
	public interface IServicioAdmin
	{
		Task<EstadisticasGlobales> ObtenerEstadisticasGlobales();
		
		Task<List<UsuarioAdmin>> ObtenerTodosUsuarios();
		Task<UsuarioAdmin?> ObtenerUsuarioPorId(int id);
		Task<bool> EliminarUsuario(int usuarioId);
		Task<Usuario?> ObtenerUsuarioCompleto(int usuarioId);
		Task<List<UsuarioAdmin>> ObtenerUsuariosEliminados();
		Task<bool> RestaurarUsuario(int usuarioId);
		Task<bool> EliminarUsuarioPermanente(int usuarioId);
		
		Task<bool> DarFichas(int usuarioId, int cantidad);
		Task<bool> QuitarFichas(int usuarioId, int cantidad);	
		Task<List<ArticuloTienda>> ObtenerTodosArticulos();
		Task<ArticuloTienda?> ObtenerArticuloPorId(int articuloId);
		Task<bool> CrearArticulo(ArticuloTienda articulo);
		Task<bool> EditarArticulo(ArticuloTienda articulo);
		Task<bool> CambiarEstadoArticulo(int articuloId, bool disponible);
	}

	public class ServicioAdmin : IServicioAdmin
	{
		private readonly ContextoBaseDatos _contexto;

		public ServicioAdmin(ContextoBaseDatos contexto)
		{
			_contexto = contexto;
		}
		
		//	Organisacion : Estadisticas (dashboard)
		public async Task<EstadisticasGlobales> ObtenerEstadisticasGlobales()
		{
			var totalUsuarios = await _contexto.Usuarios.CountAsync();
			var totalPartidas = await _contexto.Partidas.CountAsync();
			var totalFichas = await _contexto.Usuarios.SumAsync(u => u.FichasDisponibles);
			var totalCompras = await _contexto.ComprasUsuarios.CountAsync();
			
			// Nota por si lo olvidas: Lo calculamos sumando los precios de los articulos
			var fichasGastadas = await _contexto.ComprasUsuarios
				.Include(c => c.Articulo)
				.SumAsync(c => c.Articulo.PrecioFichas);

			return new EstadisticasGlobales
			{
				TotalUsuarios = totalUsuarios,
				TotalPartidas = totalPartidas,
				TotalFichasCirculacion = totalFichas,
				TotalCompras = totalCompras,
				TotalFichasGastadas = fichasGastadas,
				FechaActualizacion = DateTime.Now
			};
		}

		public async Task<List<UsuarioAdmin>> ObtenerTodosUsuarios()
		{
			var usuarios = await _contexto.Usuarios
				.Where(u => u.Rol == "user" && !u.EstaEliminado)
				.Select(u => new UsuarioAdmin
				{
					Id = u.Id,
					Nombre = u.Nombre,
					Email = u.Email,
					FichasDisponibles = u.FichasDisponibles,
					FechaCreacion = u.FechaCreacion,
					UltimoAcceso = u.UltimoAcceso,
					TotalPartidas = 0,
					TotalCompras = _contexto.ComprasUsuarios.Count(c => c.UsuarioId == u.Id)
				})
				.OrderByDescending(u => u.FechaCreacion)
				.ToListAsync();

			return usuarios;
		}

		public async Task<UsuarioAdmin?> ObtenerUsuarioPorId(int id)
		{
			var usuario = await _contexto.Usuarios
				.Where(u => u.Id == id)
				.Select(u => new UsuarioAdmin
				{
					Id = u.Id,
					Nombre = u.Nombre,
					Email = u.Email,
					FichasDisponibles = u.FichasDisponibles,
					FechaCreacion = u.FechaCreacion,
					UltimoAcceso = u.UltimoAcceso,
					TotalPartidas = 0,
					TotalCompras = _contexto.ComprasUsuarios.Count(c => c.UsuarioId == u.Id)
				})
				.FirstOrDefaultAsync();

			return usuario;
		}
		
		// Orhanizacion: Fichas
		public async Task<bool> DarFichas(int usuarioId, int cantidad)
		{
			var usuario = await _contexto.Usuarios.FindAsync(usuarioId);
			if (usuario == null) return false;

			usuario.FichasDisponibles += cantidad;
			await _contexto.SaveChangesAsync();
			return true;
		}

		public async Task<bool> QuitarFichas(int usuarioId, int cantidad)
		{
			var usuario = await _contexto.Usuarios.FindAsync(usuarioId);
			if (usuario == null) return false;

			usuario.FichasDisponibles -= cantidad;
			
			if (usuario.FichasDisponibles < 0)
				usuario.FichasDisponibles = 0;

			await _contexto.SaveChangesAsync();
			return true;
		}
		
		//	organizacion: Usuario
		public async Task<bool> EliminarUsuario(int usuarioId)
		{
			var usuario = await _contexto.Usuarios.FindAsync(usuarioId);
			if (usuario == null || usuario.Rol == "admin") return false;
			
			usuario.EstaEliminado = true;
			usuario.FechaEliminacion = DateTime.Now;
			
			await _contexto.SaveChangesAsync();
			return true;
		}

		public async Task<Usuario?> ObtenerUsuarioCompleto(int usuarioId)
		{
			return await _contexto.Usuarios.FindAsync(usuarioId);
		}
		
		public async Task<List<UsuarioAdmin>> ObtenerUsuariosEliminados()
		{
			var usuarios = await _contexto.Usuarios
				.Where(u => u.Rol == "user" && u.EstaEliminado) // Solo eliminados
				.Select(u => new UsuarioAdmin
				{
					Id = u.Id,
					Nombre = u.Nombre,
					Email = u.Email,
					FichasDisponibles = u.FichasDisponibles,
					FechaCreacion = u.FechaCreacion,
					UltimoAcceso = u.UltimoAcceso,
					TotalPartidas = 0,
					TotalCompras = _contexto.ComprasUsuarios.Count(c => c.UsuarioId == u.Id)
				})
				.OrderByDescending(u => u.FechaCreacion)
				.ToListAsync();

			return usuarios;
		}

		public async Task<bool> RestaurarUsuario(int usuarioId)
		{
			var usuario = await _contexto.Usuarios.FindAsync(usuarioId);
			if (usuario == null || !usuario.EstaEliminado) return false;

			usuario.EstaEliminado = false;
			usuario.FechaEliminacion = null;
			
			await _contexto.SaveChangesAsync();
			return true;
		}
		
		public async Task<bool> EliminarUsuarioPermanente(int usuarioId)
		{
			var usuario = await _contexto.Usuarios.FindAsync(usuarioId);
			if (usuario == null || usuario.Rol == "admin") return false;

			var compras = await _contexto.ComprasUsuarios
				.Where(c => c.UsuarioId == usuarioId)
				.ToListAsync();
			
			_contexto.ComprasUsuarios.RemoveRange(compras);

			_contexto.Usuarios.Remove(usuario);
			await _contexto.SaveChangesAsync();
			return true;
		}
		
		// Organizacion: tienda
		public async Task<List<ArticuloTienda>> ObtenerTodosArticulos()
		{
			return await _contexto.ArticulosTienda
				.OrderByDescending(a => a.Id)
				.ToListAsync();
		}
		
		public async Task<ArticuloTienda?> ObtenerArticuloPorId(int articuloId)
		{
			return await _contexto.ArticulosTienda.FindAsync(articuloId);
		}
		
		public async Task<bool> CrearArticulo(ArticuloTienda articulo)
		{
			try
			{
				_contexto.ArticulosTienda.Add(articulo);
				await _contexto.SaveChangesAsync();
				return true;
			}
			catch
			{
				return false;
			}
		}
		
		public async Task<bool> EditarArticulo(ArticuloTienda articulo)
		{
			try
			{
				var articuloExistente = await _contexto.ArticulosTienda.FindAsync(articulo.Id);
				if (articuloExistente == null) return false;

				articuloExistente.Nombre = articulo.Nombre;
				articuloExistente.Descripcion = articulo.Descripcion;
				articuloExistente.PrecioFichas = articulo.PrecioFichas;
				articuloExistente.TipoArticulo = articulo.TipoArticulo;
				articuloExistente.ImagenUrl = articulo.ImagenUrl;

				await _contexto.SaveChangesAsync();
				return true;
			}
			catch
			{
				return false;
			}
		}
		
		public async Task<bool> CambiarEstadoArticulo(int articuloId, bool disponible)
		{
			try
			{
				var articulo = await _contexto.ArticulosTienda.FindAsync(articuloId);
				if (articulo == null) return false;

				articulo.Disponible = disponible;
				await _contexto.SaveChangesAsync();
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}