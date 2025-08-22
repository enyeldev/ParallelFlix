namespace Api.Data;

using Api.Domain.Models;
using Microsoft.EntityFrameworkCore;

public class ContextoAplicacion : DbContext {
    public ContextoAplicacion(DbContextOptions<ContextoAplicacion> opciones) : base(opciones) {}

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Pelicula> Peliculas => Set<Pelicula>();
    public DbSet<Genero> Generos => Set<Genero>();
    public DbSet<PeliculaGenero> PeliculaGeneros => Set<PeliculaGenero>();
    public DbSet<Calificacion> Calificaciones => Set<Calificacion>();
    public DbSet<Tendencia> Tendencias => Set<Tendencia>();

    protected override void OnModelCreating(ModelBuilder mb) {
        mb.Entity<PeliculaGenero>().HasKey(x => new { x.PeliculaId, x.GeneroId });
        mb.Entity<Calificacion>().HasKey(x => new { x.UsuarioId, x.PeliculaId });
        mb.Entity<Usuario>().HasIndex(x => x.Correo).IsUnique();
        base.OnModelCreating(mb);
    }
}

