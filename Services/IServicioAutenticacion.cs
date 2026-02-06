using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using BlackJackMVC.Models;
using BlackJackMVC.Data;

namespace BlackJackMVC.Services
{
	public interface IServicioAutenticacion
	{
		Task<ResultadoAutenticacion> RegistrarUsuario(ModeloRegistro modelo);
		Task<ResultadoAutenticacion> IniciarSesion(ModeloLogin modelo);
		Task<Usuario?> ObtenerUsuarioPorId(int id);
		Task ActualizarUltimoAcceso(int usuarioId);
	}

	public class ServicioAutenticacion : IServicioAutenticacion
	{
		private readonly ContextoBaseDatos _contexto;

		public ServicioAutenticacion(ContextoBaseDatos contexto)
		{
			_contexto = contexto;
		}

		public async Task<ResultadoAutenticacion> RegistrarUsuario(ModeloRegistro modelo)
		{
			var usuarioExistente = await _contexto.Usuarios
				.FirstOrDefaultAsync(u => u.Nombre == modelo.NombreUsuario);

			if (usuarioExistente != null)
			{
				return new ResultadoAutenticacion
				{
					Exitoso = false,
					Mensaje = "El nombre de usuario ya está en uso"
				};
			}

			var emailExistente = await _contexto.Usuarios
				.FirstOrDefaultAsync(u => u.Email == modelo.Email);

			if (emailExistente != null)
			{
				return new ResultadoAutenticacion
				{
					Exitoso = false,
					Mensaje = "El email ya está registrado"
				};
			}

			var nuevoUsuario = new Usuario
			{
				Nombre = modelo.NombreUsuario,
				Email = modelo.Email,
				ContrasenaHash = HashearContrasena(modelo.Contrasena),
				FichasDisponibles = 1000, // Fichas gratis pa q no empieze pobre xd
				Rol = "user",
				FechaCreacion = DateTime.Now,
				UltimoAcceso = DateTime.Now
			};

			_contexto.Usuarios.Add(nuevoUsuario);
			await _contexto.SaveChangesAsync();

			return new ResultadoAutenticacion
			{
				Exitoso = true,
				Mensaje = "Usuario registrado exitosamente",
				Usuario = nuevoUsuario
			};
		}

		public async Task<ResultadoAutenticacion> IniciarSesion(ModeloLogin modelo)
		{
			var usuario = await _contexto.Usuarios
				.FirstOrDefaultAsync(u => u.Nombre == modelo.NombreUsuario);

			if (usuario == null)
			{
				return new ResultadoAutenticacion
				{
					Exitoso = false,
					Mensaje = "Usuario o contraseña incorrectos"
				};
			}
			
			if (usuario.EstaEliminado)
			{
				return new ResultadoAutenticacion
				{
					Exitoso = false,
					Mensaje = "Esta cuenta ha sido desactivada. Contacta al administrador."
				};
			}
	
			if (!VerificarContrasena(modelo.Contrasena, usuario.ContrasenaHash))
			{
				return new ResultadoAutenticacion
				{
					Exitoso = false,
					Mensaje = "Usuario o contraseña incorrectos"
				};
			}

			usuario.UltimoAcceso = DateTime.Now;
			await _contexto.SaveChangesAsync();

			return new ResultadoAutenticacion
			{
				Exitoso = true,
				Mensaje = "Inicio de sesion exitoso",
				Usuario = usuario
			};
		}

		public async Task<Usuario?> ObtenerUsuarioPorId(int id)
		{
			return await _contexto.Usuarios.FindAsync(id);
		}

		public async Task ActualizarUltimoAcceso(int usuarioId)
		{
			var usuario = await _contexto.Usuarios.FindAsync(usuarioId);
			if (usuario != null)
			{
				usuario.UltimoAcceso = DateTime.Now;
				await _contexto.SaveChangesAsync();
			}
		}
		// Aqui esta el hasheo de contraseña
		private string HashearContrasena(string contrasena)
		{
			using (var sha256 = SHA256.Create())
			{
				var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(contrasena));
				var builder = new StringBuilder();
				foreach (var b in bytes)
				{
					builder.Append(b.ToString("x2"));
				}
				return builder.ToString();
			}
		}

		private bool VerificarContrasena(string contrasena, string hash)
		{
			var hashDeContrasena = HashearContrasena(contrasena);
			return hashDeContrasena == hash;
		}
	}
}