using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using BlackJackMVC.Data;
using BlackJackMVC.Models;

namespace BlackJackMVC.Services
{
	public interface IServicioTienda
	{
		Task<VistaTienda> ObtenerDatosTienda(int usuarioId);
		Task<ResultadoCompra> ComprarArticulo(int usuarioId, int articuloId);
		Task<int> ObtenerFichasUsuario(int usuarioId);
	}

	public class ServicioTienda : IServicioTienda
	{
		private readonly ContextoBaseDatos _contexto;

		public ServicioTienda(ContextoBaseDatos contexto)
		{
			_contexto = contexto;
		}

		public async Task<VistaTienda> ObtenerDatosTienda(int usuarioId)
		{
			var usuario = await _contexto.Usuarios.FindAsync(usuarioId);
			if (usuario == null)
				throw new Exception("Usuario no encontrado");

			var todosLosArticulos = await _contexto.ArticulosTienda
				.Where(a => a.Disponible)
				.ToListAsync();

			var idsArticulosComprados = await _contexto.ComprasUsuarios
				.Where(c => c.UsuarioId == usuarioId)
				.Select(c => c.ArticuloId)
				.ToListAsync();

			var articulosComprados = todosLosArticulos
				.Where(a => idsArticulosComprados.Contains(a.Id))
				.ToList();

			var articulosDisponibles = todosLosArticulos
				.Where(a => !idsArticulosComprados.Contains(a.Id))
				.ToList();

			return new VistaTienda
			{
				UsuarioActual = usuario,
				ArticulosDisponibles = articulosDisponibles,
				ArticulosComprados = articulosComprados
			};
		}

		// Hay q procesar la compra de un articulo verificando fichas y duplicados
		public async Task<ResultadoCompra> ComprarArticulo(int usuarioId, int articuloId)
		{
			var usuario = await _contexto.Usuarios.FindAsync(usuarioId);
			var articulo = await _contexto.ArticulosTienda.FindAsync(articuloId);

			if (usuario == null || articulo == null)
			{
				return new ResultadoCompra
				{
					Exitosa = false,
					Mensaje = "Usuario o articulo no encontrado"
				};
			}

			var yaComprado = await _contexto.ComprasUsuarios
				.AnyAsync(c => c.UsuarioId == usuarioId && c.ArticuloId == articuloId);

			if (yaComprado)
			{
				return new ResultadoCompra
				{
					Exitosa = false,
					Mensaje = "Ya tienes este articulo",
					FichasRestantes = usuario.FichasDisponibles
				};
			}

			if (usuario.FichasDisponibles < articulo.PrecioFichas)
			{
				return new ResultadoCompra
				{
					Exitosa = false,
					Mensaje = $"No tienes suficientes fichas. Necesitas {articulo.PrecioFichas} fichas",
					FichasRestantes = usuario.FichasDisponibles
				};
			}
			
			usuario.FichasDisponibles -= articulo.PrecioFichas;

			if (articulo.TipoArticulo == "bonus")
			{
				usuario.FichasDisponibles += 500;
			}

			var compra = new CompraUsuario
			{
				UsuarioId = usuarioId,
				ArticuloId = articuloId,
				FechaCompra = DateTime.Now
			};

			_contexto.ComprasUsuarios.Add(compra);
			await _contexto.SaveChangesAsync();

			return new ResultadoCompra
			{
				Exitosa = true,
				Mensaje = $"¡Compraste {articulo.Nombre}!",
				FichasRestantes = usuario.FichasDisponibles
			};
		}

		public async Task<int> ObtenerFichasUsuario(int usuarioId)
		{
			var usuario = await _contexto.Usuarios.FindAsync(usuarioId);
			return usuario?.FichasDisponibles ?? 0;
		}
	}
}