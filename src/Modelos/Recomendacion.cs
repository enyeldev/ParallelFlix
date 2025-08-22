using System;

namespace src.Modelos;

public class Recomendacion
{
    public Pelicula Pelicula { get; init; } = new();
    public double Puntuacion { get; init; }
}
