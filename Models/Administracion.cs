namespace BlackJackMVC.Models
{
	public class EstadisticasGlobales
	{
		public int TotalUsuarios { get; set; }
		public int TotalPartidas { get; set; }
		public int TotalFichasCirculacion { get; set; }
		public int TotalCompras { get; set; }
		public int TotalFichasGastadas { get; set; }
		public DateTime FechaActualizacion { get; set; }
	}

	public class UsuarioAdmin
	{
		public int Id { get; set; }
		public string Nombre { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public int FichasDisponibles { get; set; }
		public DateTime FechaCreacion { get; set; }
		public DateTime? UltimoAcceso { get; set; }
		public int TotalPartidas { get; set; }
		public int TotalCompras { get; set; }
	}

	public class DashboardAdmin
	{
		public EstadisticasGlobales Estadisticas { get; set; } = new EstadisticasGlobales();
		public List<UsuarioAdmin> UltimosUsuarios { get; set; } = new List<UsuarioAdmin>();
	}
	
	public class ArticuloConEstadisticas
	{
		public int Id { get; set; }
		public string Nombre { get; set; } = string.Empty;
		public string Descripcion { get; set; } = string.Empty;
		public int PrecioFichas { get; set; }
		public string TipoArticulo { get; set; } = string.Empty;
		public string ImagenUrl { get; set; } = string.Empty;
		public bool Disponible { get; set; }
		public int TotalVendidos { get; set; }
		public int FichasGeneradas { get; set; }
	}
}