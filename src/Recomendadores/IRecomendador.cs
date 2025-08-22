using src.Modelos;

namespace src.Recomendadores;

public interface IRecomendador
{
    string Nombre { get; }
    Task<List<Recomendacion>> RecomendarAsync(PerfilUsuario usuario, List<Pelicula> corpus, int k, CancellationToken ct);
}
