// tests/SimilitudTests.cs
using Xunit;
using src.Modelos;
using src.Nucleo;

public class SimilitudTests
{
    [Fact]
    public void JaccardGenero_GeneroIgual_Retorna1()
    {
        var p1 = new Pelicula { Id = 1, Genero = "Accion" };
        var p2 = new Pelicula { Id = 2, Genero = "Accion" };
        double sim = Similitud.JaccardGenero(p1, p2);
        Assert.Equal(1.0, sim);
    }

    [Fact]
    public void JaccardGenero_GeneroDistinto_Retorna025()
    {
        var p1 = new Pelicula { Id = 1, Genero = "Accion" };
        var p2 = new Pelicula { Id = 2, Genero = "Comedia" };
        double sim = Similitud.JaccardGenero(p1, p2);
        Assert.Equal(0.25, sim);
    }

    [Fact]
    public void JaccardEtiquetas_InterseccionCorrecta_RetornaValorCorrecto()
    {
        var p1 = new Pelicula { Etiquetas = new List<string> { "Aventura", "Heroe" } };
        var p2 = new Pelicula { Etiquetas = new List<string> { "Heroe", "Magia" } };

        double sim = Similitud.JaccardEtiquetas(p1, p2);


        Assert.Equal(1.0 / 3.0, sim, 5);
    }

    [Fact]
    public void JaccardEtiquetas_SinEtiquetas_Retorna0()
    {
        var p1 = new Pelicula { Etiquetas = new List<string>() };
        var p2 = new Pelicula { Etiquetas = new List<string>() };

        double sim = Similitud.JaccardEtiquetas(p1, p2);

        Assert.Equal(0, sim);
    }

    [Fact]
    public void SimilitudCacheada_RetornaMismaSimilitudSiSeLlamaDosVeces()
    {
        var p1 = new Pelicula { Id = 1, Genero = "Accion", Etiquetas = new List<string> { "Aventura" } };
        var p2 = new Pelicula { Id = 2, Genero = "Accion", Etiquetas = new List<string> { "Aventura" } };

        double sim1 = Similitud.SimilitudCacheada(p1, p2);
        double sim2 = Similitud.SimilitudCacheada(p1, p2);

        Assert.Equal(sim1, sim2);
    }


}
