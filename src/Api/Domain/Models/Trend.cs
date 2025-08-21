using System;

namespace Api.Domain.Models;

public class Trend
{
    public int Id { get; set; }
    public int PeliculaId { get; set; }
    public Movie Pelicula { get; set; } = default!;
    public float GlobalHotness { get; set; }
    public DateTime UpdatedAt { get; set; }
}
