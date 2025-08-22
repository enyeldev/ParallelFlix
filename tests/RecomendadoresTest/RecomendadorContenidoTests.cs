using Xunit;
using src.Modelos;
using src.Recomendadores;
using src.Nucleo;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class RecomendadorContenidoTests
{
    private List<Pelicula> CrearPeliculasPrueba()
    {
        return new List<Pelicula>
        {
            new Pelicula { Id = 1, Titulo = "Accion 1", Genero = "Accion", Calificacion = 4.5, Etiquetas = new List<string>{ "Heroe", "Aventura" } },
            new Pelicula { Id = 2, Titulo = "Comedia 1", Genero = "Comedia", Calificacion = 3.0, Etiquetas = new List<string>{ "Humor" } },
            new Pelicula { Id = 3, Titulo = "Accion 2", Genero = "Accion", Calificacion = 4.0, Etiquetas = new List<string>{ "Aventura" } },
            new Pelicula { Id = 4, Titulo = "Drama 1", Genero = "Drama", Calificacion = 2.5, Etiquetas = new List<string>{ "Romance" } }
        };
    }

    private PerfilUsuario CrearUsuarioPrueba()
    {
        var usuario = new PerfilUsuario("usuario1");
        usuario.IdsVistos.Add(1); // ya vio la primera película
        usuario.GenerosPreferidos.Add("Accion");
        usuario.GenerosPreferidos.Add("Comedia");
        usuario.EtiquetasPreferidas.Add("Aventura");
        usuario.EtiquetasPreferidas.Add("Heroe");
        return usuario;
    }

    [Fact]
    public async Task RecomendarAsync_RetornaKRecomendaciones_SinRepetidos()
    {
        var corpus = CrearPeliculasPrueba();
        var usuario = CrearUsuarioPrueba();
        var recomendador = new RecomendadorContenido();

        int k = 2;
        var ct = CancellationToken.None;
        var resultado = await recomendador.RecomendarAsync(usuario, corpus, k, ct);

        // Debe devolver exactamente k recomendaciones
        Assert.Equal(k, resultado.Count);

        // Ninguna recomendación debe ser de una película ya vista
        Assert.All(resultado, r => Assert.DoesNotContain(r.Pelicula.Id, usuario.IdsVistos));
    }

    [Fact]
    public async Task RecomendarAsync_OrdenadasPorPuntuacionDescendente()
    {
        var corpus = CrearPeliculasPrueba();
        var usuario = CrearUsuarioPrueba();
        var recomendador = new RecomendadorContenido();

        int k = 3;
        var ct = CancellationToken.None;
        var resultado = await recomendador.RecomendarAsync(usuario, corpus, k, ct);

        // Verifica que estén ordenadas descendente por puntuación
        for (int i = 0; i < resultado.Count - 1; i++)
        {
            Assert.True(resultado[i].Puntuacion >= resultado[i + 1].Puntuacion);
        }
    }

    [Fact]
    public async Task RecomendarAsync_KMayorQueDisponibles_RetornaTodasDisponibles()
    {
        var corpus = CrearPeliculasPrueba();
        var usuario = CrearUsuarioPrueba();
        var recomendador = new RecomendadorContenido();

        int k = 10; // más grande que el número de candidatos
        var ct = CancellationToken.None;
        var resultado = await recomendador.RecomendarAsync(usuario, corpus, k, ct);

        // Debe devolver solo las películas no vistas
        int esperadas = corpus.Count - usuario.IdsVistos.Count;
        Assert.Equal(esperadas, resultado.Count);
    }
}
