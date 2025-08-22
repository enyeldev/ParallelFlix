using System;

namespace src.Modelos;

public class Pelicula
{
    public int Id { get; init; }
    public string Titulo { get; init; } = string.Empty;
    public string Genero { get; init; } = string.Empty;
    public List<string> Etiquetas { get; init; } = new();
    public double Calificacion { get; init; }
    public string Duracion { get; init; } = string.Empty;
    public int Ano { get; init; }
    public string Director { get; init; } = string.Empty;
}
