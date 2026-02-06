using System.Text.Json;
using BlackJackMVC.Data;
using BlackJackMVC.Models;

namespace BlackJackMVC.Services
{
	public interface IServicioJuego
	{
		Task<VistaJuegoBlackjack> IniciarNuevaPartida(int usuarioId, int apuesta);
		Task<VistaJuegoBlackjack> JugadorPideCarta(int idPartida);
		Task<VistaJuegoBlackjack> JugadorSePlanta(int idPartida);
		Task<VistaJuegoBlackjack> ObtenerPartida(int idPartida);
	}

	public class ServicioJuego : IServicioJuego
	{
		private readonly ContextoBaseDatos _contexto;

		public ServicioJuego(ContextoBaseDatos contexto)
		{
			_contexto = contexto;
		}

		public async Task<VistaJuegoBlackjack> IniciarNuevaPartida(int usuarioId, int apuesta)
		{
			var usuario = await _contexto.Usuarios.FindAsync(usuarioId);
			if (usuario == null)
				throw new Exception("Usuario no encontrado");

			if (usuario.FichasDisponibles < apuesta)
				throw new Exception("No tienes suficientes fichas para esta apuesta");

			usuario.FichasDisponibles -= apuesta;

			var barajaCompleta = CrearYMezclarBaraja();
			
			var cartasJugador = new List<Carta> { barajaCompleta[0], barajaCompleta[2] };
			var cartasDealer = new List<Carta> { barajaCompleta[1], barajaCompleta[3] };
			
			barajaCompleta.RemoveRange(0, 4);

			var partidaNueva = new PartidaBlackjack
			{
				UsuarioId = usuarioId,
				ApuestaFichas = apuesta,
				GananciaFichas = 0,
				CartasJugadorJson = JsonSerializer.Serialize(cartasJugador),
				CartasDealerJson = JsonSerializer.Serialize(cartasDealer),
				BarajaRestanteJson = JsonSerializer.Serialize(barajaCompleta),
				EstadoPartida = "jugando",
				MensajeResultado = "¿Pedir carta o plantarse?",
				FechaCreacion = DateTime.Now
			};

			if (CalcularTotalMano(cartasJugador) == 21)
			{
				partidaNueva.EstadoPartida = "terminado";
				partidaNueva.MensajeResultado = "BLACKJACK! Ganaste";
				
				int ganancia = (int)(apuesta * 1.5);
				partidaNueva.GananciaFichas = ganancia;
				usuario.FichasDisponibles += apuesta + ganancia;
			}

			_contexto.Partidas.Add(partidaNueva);
			await _contexto.SaveChangesAsync();

			return ConvertirPartidaAVista(partidaNueva, usuario.FichasDisponibles);
		}

		public async Task<VistaJuegoBlackjack> JugadorPideCarta(int idPartida)
		{
			var partida = await _contexto.Partidas.FindAsync(idPartida);
			
			if (partida == null || partida.EstadoPartida != "jugando")
				return null;

			var cartasJugador = JsonSerializer.Deserialize<List<Carta>>(partida.CartasJugadorJson);
			var barajaRestante = JsonSerializer.Deserialize<List<Carta>>(partida.BarajaRestanteJson);

			cartasJugador.Add(barajaRestante[0]);
			barajaRestante.RemoveAt(0);

			var totalJugador = CalcularTotalMano(cartasJugador);

			if (totalJugador > 21)
			{
				partida.EstadoPartida = "terminado";
				partida.MensajeResultado = "Te pasaste de 21. Perdiste";
				partida.GananciaFichas = -partida.ApuestaFichas;
			}
			
			else if (totalJugador == 21)
			{
				partida.CartasJugadorJson = JsonSerializer.Serialize(cartasJugador);
				partida.BarajaRestanteJson = JsonSerializer.Serialize(barajaRestante);
				await _contexto.SaveChangesAsync();
				
				return await JugadorSePlanta(idPartida);
			}

			partida.CartasJugadorJson = JsonSerializer.Serialize(cartasJugador);
			partida.BarajaRestanteJson = JsonSerializer.Serialize(barajaRestante);
			partida.FechaActualizacion = DateTime.Now;

			await _contexto.SaveChangesAsync();

			var usuario = await _contexto.Usuarios.FindAsync(partida.UsuarioId);
			return ConvertirPartidaAVista(partida, usuario?.FichasDisponibles ?? 0);
		}

		public async Task<VistaJuegoBlackjack> JugadorSePlanta(int idPartida)
		{
			var partida = await _contexto.Partidas.FindAsync(idPartida);
			
			if (partida == null || partida.EstadoPartida != "jugando")
				return null;

			var usuario = await _contexto.Usuarios.FindAsync(partida.UsuarioId);
			if (usuario == null)
				return null;

			var cartasDealer = JsonSerializer.Deserialize<List<Carta>>(partida.CartasDealerJson);
			var barajaRestante = JsonSerializer.Deserialize<List<Carta>>(partida.BarajaRestanteJson);

			while (CalcularTotalMano(cartasDealer) < 17)
			{
				cartasDealer.Add(barajaRestante[0]);
				barajaRestante.RemoveAt(0);
			}

			var totalJugador = CalcularTotalMano(JsonSerializer.Deserialize<List<Carta>>(partida.CartasJugadorJson));
			var totalDealer = CalcularTotalMano(cartasDealer);

			partida.EstadoPartida = "terminado";
			
			if (totalDealer > 21)
			{
				partida.MensajeResultado = "Dealer se pasó de 21. Ganaste!";
				int ganancia = partida.ApuestaFichas;
				partida.GananciaFichas = ganancia;
				usuario.FichasDisponibles += partida.ApuestaFichas + ganancia;
			}
			else if (totalJugador > totalDealer)
			{
				partida.MensajeResultado = "Ganaste!";
				int ganancia = partida.ApuestaFichas;
				partida.GananciaFichas = ganancia;
				usuario.FichasDisponibles += partida.ApuestaFichas + ganancia;
			}
			else if (totalJugador < totalDealer)
			{
				partida.MensajeResultado = "Perdiste";
				partida.GananciaFichas = -partida.ApuestaFichas;
			}
			else
			{
				partida.MensajeResultado = "Empate";
				partida.GananciaFichas = 0;
				usuario.FichasDisponibles += partida.ApuestaFichas;
			}

			partida.CartasDealerJson = JsonSerializer.Serialize(cartasDealer);
			partida.BarajaRestanteJson = JsonSerializer.Serialize(barajaRestante);
			partida.FechaActualizacion = DateTime.Now;

			await _contexto.SaveChangesAsync();

			return ConvertirPartidaAVista(partida, usuario.FichasDisponibles);
		}

		public async Task<VistaJuegoBlackjack> ObtenerPartida(int idPartida)
		{
			var partida = await _contexto.Partidas.FindAsync(idPartida);
			if (partida == null)
				return null;

			var usuario = await _contexto.Usuarios.FindAsync(partida.UsuarioId);
			return ConvertirPartidaAVista(partida, usuario?.FichasDisponibles ?? 0);
		}

		private List<Carta> CrearYMezclarBaraja()
		{
			var palos = new[] { "♠", "♥", "♦", "♣" };
			var valores = new[] { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };
			var barajaCompleta = new List<Carta>();

			foreach (var palo in palos)
			{
				foreach (var valor in valores)
				{
					barajaCompleta.Add(new Carta { Palo = palo, Valor = valor });
				}
			}

			var generadorAleatorio = new Random();
			for (int i = barajaCompleta.Count - 1; i > 0; i--)
			{
				int j = generadorAleatorio.Next(i + 1);
				var temporal = barajaCompleta[i];
				barajaCompleta[i] = barajaCompleta[j];
				barajaCompleta[j] = temporal;
			}

			return barajaCompleta;
		}

		private int CalcularTotalMano(List<Carta> cartas)
		{
			int total = 0;
			int cantidadAses = 0;

			foreach (var carta in cartas)
			{
				total += carta.ObtenerValorNumerico();
				if (carta.Valor == "A") cantidadAses++;
			}

			while (total > 21 && cantidadAses > 0)
			{
				total -= 10;
				cantidadAses--;
			}

			return total;
		}

		private VistaJuegoBlackjack ConvertirPartidaAVista(PartidaBlackjack partida, int fichasUsuario)
		{
			var cartasJugador = JsonSerializer.Deserialize<List<Carta>>(partida.CartasJugadorJson);
			var cartasDealer = JsonSerializer.Deserialize<List<Carta>>(partida.CartasDealerJson);

			return new VistaJuegoBlackjack
			{
				IdPartida = partida.Id,
				CartasJugador = cartasJugador,
				CartasDealer = cartasDealer,
				TotalJugador = CalcularTotalMano(cartasJugador),
				TotalDealer = CalcularTotalMano(cartasDealer),
				EstadoPartida = partida.EstadoPartida,
				MensajeParaMostrar = partida.MensajeResultado,
				MostrarSegundaCartaDealer = partida.EstadoPartida != "jugando",
				ApuestaFichas = partida.ApuestaFichas, 
				GananciaFichas = partida.GananciaFichas, 
				FichasUsuario = fichasUsuario 
			};
		}
	}
}