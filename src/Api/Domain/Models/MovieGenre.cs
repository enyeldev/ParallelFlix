using System;

namespace Api.Domain.Models;

public class GeneroPelicula
{
    public int IdPelicula { get; set; }
    public Movie Pelicula { get; set; } = default!;
    public int IdGenero { get; set; }
    public Genre Genero { get; set; } = default!;
}
