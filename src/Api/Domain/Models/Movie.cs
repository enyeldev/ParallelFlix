using System;

namespace Api.Domain.Models;

public class Movie
{
    public int Id { get; set; }
    public string Titulo { get; set; } = default!;
    public ICollection<GeneroPelicula> GeneroPelicula { get; set; } = new List<GeneroPelicula>();
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
}
