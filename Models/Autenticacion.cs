using System.ComponentModel.DataAnnotations;

namespace BlackJackMVC.Models
{
	public class ModeloRegistro
	{
		[Required(ErrorMessage = "El nombre de usuario es obligatorio")]
		[StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 50 caracteres")]
		public string NombreUsuario { get; set; } = string.Empty;

		[Required(ErrorMessage = "El email es obligatorio")]
		[EmailAddress(ErrorMessage = "Email invalido")]
		public string Email { get; set; } = string.Empty;

		[Required(ErrorMessage = "La contraseña es obligatoria")]
		[StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
		public string Contrasena { get; set; } = string.Empty;

		[Required(ErrorMessage = "Confirma tu contraseña")]
		[Compare("Contrasena", ErrorMessage = "Las contraseñas no coinciden")]
		public string ConfirmarContrasena { get; set; } = string.Empty;
	}

	public class ModeloLogin
	{
		[Required(ErrorMessage = "El nombre de usuario es obligatorio")]
		public string NombreUsuario { get; set; } = string.Empty;

		[Required(ErrorMessage = "La contraseña es obligatoria")]
		public string Contrasena { get; set; } = string.Empty;

		public bool Recordarme { get; set; }
	}

	public class ResultadoAutenticacion
	{
		public bool Exitoso { get; set; }
		public string Mensaje { get; set; } = string.Empty;
		public Usuario? Usuario { get; set; }
	}
}