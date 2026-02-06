namespace BlackJackMVC.Models
{
	public class Carta
	{
		public string Palo { get; set; } // ♠, ♥, ♦, ♣
		public string Valor { get; set; } // A, 2-10, J, Q, K
		
		public int ObtenerValorNumerico()
		{
			if (Valor == "A") return 11;
			if (Valor == "J" || Valor == "Q" || Valor == "K") return 10;
			return int.Parse(Valor);
		}
	}

	// pa guardar la partida compmletada en la base de datos
	public class PartidaBlackjack
	{
		public int Id { get; set; }
		
		public int UsuarioId { get; set; }
		public int ApuestaFichas { get; set; } = 0;
		public int GananciaFichas { get; set; } = 0;
		
		// Las cartas se guardaran como JSON porq SQL Server no soporta arrays directamente
		public string CartasJugadorJson { get; set; }
		public string CartasDealerJson { get; set; }
		public string BarajaRestanteJson { get; set; }
		
		public string EstadoPartida { get; set; }
		public string MensajeResultado { get; set; }
		public DateTime FechaCreacion { get; set; }
		public DateTime? FechaActualizacion { get; set; }
	}
	
	public class VistaJuegoBlackjack
	{
		public int IdPartida { get; set; }
		public List<Carta> CartasJugador { get; set; }
		public List<Carta> CartasDealer { get; set; }
		public int TotalJugador { get; set; }
		public int TotalDealer { get; set; }
		public string EstadoPartida { get; set; }
		
		public string MensajeParaMostrar { get; set; }
		
		public bool MostrarSegundaCartaDealer { get; set; }
		
		public int ApuestaFichas { get; set; }
		public int GananciaFichas { get; set; }
		public int FichasUsuario { get; set; }
	}
}