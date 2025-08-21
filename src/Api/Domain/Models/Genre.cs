using System;

namespace Api.Domain.Models;

public class Genre
{
    public int Id { get; set; }
    public string Nombre { get; set; } = default!;
    public ICollection<GeneroPelicula> GeneroPelicula { get; set; } = new List<GeneroPelicula>();
}

