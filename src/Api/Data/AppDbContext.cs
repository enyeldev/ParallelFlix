using System;
using Api.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public class AppDbContext : DbContext
{

    public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<GeneroPelicula> GenerosPelicula => Set<GeneroPelicula>();
    public DbSet<Rating> Ratings => Set<Rating>();
    public DbSet<Trend> Trends => Set<Trend>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<GeneroPelicula>().HasKey(x => new { x.IdPelicula, x.IdGenero });
        mb.Entity<Rating>().HasKey(x => new { x.IdUsuario, x.IdPelicula });

        mb.Entity<User>().HasIndex(x => x.Email).IsUnique();

        base.OnModelCreating(mb);
    }
}
