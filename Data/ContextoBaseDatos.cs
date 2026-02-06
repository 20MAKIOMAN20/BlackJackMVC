using Microsoft.EntityFrameworkCore;
using BlackJackMVC.Models;

namespace BlackJackMVC.Data
{
	// Esta es la clase q maneja la configuracion de la base de datos
	public class ContextoBaseDatos : DbContext
	{
		public ContextoBaseDatos(DbContextOptions<ContextoBaseDatos> opciones)
			: base(opciones)
		{
		}

		public DbSet<PartidaBlackjack> Partidas { get; set; }
		public DbSet<Usuario> Usuarios { get; set; }
		public DbSet<ArticuloTienda> ArticulosTienda { get; set; }
		public DbSet<CompraUsuario> ComprasUsuarios { get; set; }

		protected override void OnModelCreating(ModelBuilder constructorModelo)
		{
			base.OnModelCreating(constructorModelo);
			
			constructorModelo.Entity<PartidaBlackjack>(entidad =>
			{
				entidad.HasKey(e => e.Id);
				entidad.Property(e => e.EstadoPartida).HasMaxLength(50);
				entidad.Property(e => e.MensajeResultado).HasMaxLength(200);
				entidad.Property(e => e.UsuarioId).IsRequired();
				entidad.Property(e => e.ApuestaFichas).HasDefaultValue(0);
				entidad.Property(e => e.GananciaFichas).HasDefaultValue(0);
				
				entidad.HasOne<Usuario>()
					.WithMany()
					.HasForeignKey(e => e.UsuarioId)
					.OnDelete(DeleteBehavior.Cascade);
			});

			constructorModelo.Entity<Usuario>(entidad =>
			{
				entidad.HasKey(e => e.Id);
				entidad.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
				
				// Email unico pa evitar duplicados
				entidad.HasIndex(e => e.Email).IsUnique();
				entidad.Property(e => e.Email).HasMaxLength(255).IsRequired();
				
				// Nombre de usuario unico pa evitar los duplicados
				entidad.HasIndex(e => e.Nombre).IsUnique();
				
				entidad.Property(e => e.ContrasenaHash).HasMaxLength(500).IsRequired();
				entidad.Property(e => e.FichasDisponibles).HasDefaultValue(1000);
				entidad.Property(e => e.Rol).HasMaxLength(20).IsRequired().HasDefaultValue("user");
				entidad.Property(e => e.EstaEliminado).HasDefaultValue(false);
			});

			constructorModelo.Entity<ArticuloTienda>(entidad =>
			{
				entidad.HasKey(e => e.Id);
				entidad.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
				entidad.Property(e => e.Descripcion).HasMaxLength(500);
				entidad.Property(e => e.TipoArticulo).HasMaxLength(50);
			});

			constructorModelo.Entity<CompraUsuario>(entidad =>
			{
				entidad.HasKey(e => e.Id);
				
				entidad.HasOne(e => e.Usuario)
					.WithMany()
					.HasForeignKey(e => e.UsuarioId);
				
				entidad.HasOne(e => e.Articulo)
					.WithMany()
					.HasForeignKey(e => e.ArticuloId);
			});

			// Aqui ta la tienda
			constructorModelo.Entity<ArticuloTienda>().HasData(
				new ArticuloTienda { Id = 1, Nombre = "Baraja Dorada", Descripcion = "Cartas con bordes dorados elegantes", PrecioFichas = 500, TipoArticulo = "carta", ImagenUrl = "🃏✨", Disponible = true },
				new ArticuloTienda { Id = 2, Nombre = "Baraja Neon", Descripcion = "Cartas con efecto neon brillante", PrecioFichas = 750, TipoArticulo = "carta", ImagenUrl = "🃏💫", Disponible = true },
				new ArticuloTienda { Id = 3, Nombre = "Fondo Casino VIP", Descripcion = "Mesa de blackjack premium", PrecioFichas = 1000, TipoArticulo = "fondo", ImagenUrl = "🎰", Disponible = true },
				new ArticuloTienda { Id = 4, Nombre = "Fondo Noche Estrellada", Descripcion = "Ambiente nocturno elegante", PrecioFichas = 800, TipoArticulo = "fondo", ImagenUrl = "🌙", Disponible = true },
				new ArticuloTienda { Id = 5, Nombre = "Avatar Jugador Pro", Descripcion = "Insignia de jugador profesional", PrecioFichas = 300, TipoArticulo = "avatar", ImagenUrl = "👑", Disponible = true },
				new ArticuloTienda { Id = 6, Nombre = "Bonus 500 Fichas", Descripcion = "Recibe 500 fichas extra", PrecioFichas = 100, TipoArticulo = "bonus", ImagenUrl = "💰", Disponible = true }
			);
		}
    }
}