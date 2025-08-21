public record Recomendacion(int idMovie, string titulo, float score);
public record RecResultado(string algoritmo, TimeSpan elapsed, IReadOnlyList<Recomendacion> items);

public interface IAlgoritmoRecomendacion
{
    string Nombre { get; }
    Task<RecResultado> RecomendadoAsync(int idUser, int k, CancellationToken ct);
}

public static class RecommendationQuality
{

    public static float Score(IReadOnlyList<Recomendacion> items)
        => items.Count == 0 ? 0 : items.Average(i => i.score);
}