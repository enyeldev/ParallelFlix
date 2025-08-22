using System;

namespace src.Modelos;

public class Recomendacion
{

    public int Id { get; set; }
    public int PeliculaId { get; set; }
    public Pelicula Pelicula { get; init; } = new();
    public double Puntuacion { get; init; }
}
