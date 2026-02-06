using System.ComponentModel.DataAnnotations;

namespace BlackJackMVC.Models
{
	public class Usuario
	{
		public int Id { get; set; }
		public string Nombre { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string ContrasenaHash { get; set; } = string.Empty;
		public string Rol { get; set; } = "user";
		
		public int FichasDisponibles { get; set; } = 1000;
		public DateTime FechaCreacion { get; set; }
		public DateTime? UltimoAcceso { get; set; }
		public bool EstaEliminado { get; set; } = false;
		public DateTime? FechaEliminacion { get; set; }
	}

	public class ArticuloTienda
	{
		public int Id { get; set; }
		[Required(ErrorMessage = "El Nombre es obligatorio")]
		public string Nombre { get; set; } = string.Empty;
		[Required(ErrorMessage = "La Descripcion es obligatorio")]
		public string Descripcion { get; set; } = string.Empty;
		[Required(ErrorMessage = "El Precio es obligatorio")]
		public int PrecioFichas { get; set; }
		[Required(ErrorMessage = "El Tipo es obligatorio")]
		public string TipoArticulo { get; set; } = string.Empty;
		[Required(ErrorMessage = "La Imagen es obligatoria")]
		public string ImagenUrl { get; set; } = string.Empty;
		public bool Disponible { get; set; } = true;
	}

	public class CompraUsuario
	{
		public int Id { get; set; }
		public int UsuarioId { get; set; }
		public int ArticuloId { get; set; }
		public DateTime FechaCompra { get; set; }
		public Usuario Usuario { get; set; } = null!;
		public ArticuloTienda Articulo { get; set; } = null!;
	}

	public class VistaTienda
	{
		public Usuario UsuarioActual { get; set; } = null!;
		public List<ArticuloTienda> ArticulosDisponibles { get; set; } = new List<ArticuloTienda>();
		public List<ArticuloTienda> ArticulosComprados { get; set; } = new List<ArticuloTienda>();
	}

	public class ResultadoCompra
	{
		public bool Exitosa { get; set; }
		public string Mensaje { get; set; } = string.Empty;
		public int FichasRestantes { get; set; }
	}
}