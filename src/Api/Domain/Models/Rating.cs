using System;

namespace Api.Domain.Models;

public class Rating
{
    public int IdUsuario { get; set; }
    public User Usuario { get; set; } = default!;
    public int IdPelicula { get; set; }
    public Movie Pelicula { get; set; } = default!;
    public float Score { get; set; } // 0.5..5.0
    public DateTime Timestamp { get; set; }
}
