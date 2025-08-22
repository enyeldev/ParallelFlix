using Xunit;
using src.Modelos;
using src.Servicios.AlgoritmoEspeculativo;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class MotorRecomendacionTests
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
        usuario.EtiquetasPreferidas.Add("Aventura");
        return usuario;
    }

    [Fact]
    public async Task RecomendarEspeculativoAsync_RetornaKRecomendaciones_SinVistos()
    {
        var corpus = CrearPeliculasPrueba();
        var usuario = CrearUsuarioPrueba();
        var motor = new MotorRecomendacion(corpus);

        int k = 2;
        var (resultados, tiempos) = await motor.RecomendarEspeculativoAsync(usuario, k);

        // Debe devolver exactamente k recomendaciones
        Assert.Equal(k, resultados.Count);

        // Ninguna recomendación debe ser de una película ya vista
        Assert.All(resultados, r => Assert.DoesNotContain(r.Pelicula.Id, usuario.IdsVistos));

        // Debe haber medido los tiempos de todas las estrategias
        Assert.All(new[] { "Colaborativo", "Contenido", "Tendencias" }, nombre =>
            Assert.Contains(nombre, tiempos.Keys));
    }

    [Fact]
    public async Task RecomendarEspeculativoAsync_OrdenadasPorPuntuacionDescendente()
    {
        var corpus = CrearPeliculasPrueba();
        var usuario = CrearUsuarioPrueba();
        var motor = new MotorRecomendacion(corpus);

        int k = 3;
        var (resultados, _) = await motor.RecomendarEspeculativoAsync(usuario, k);

        // Verifica que estén ordenadas descendente por puntuación
        for (int i = 0; i < resultados.Count - 1; i++)
        {
            Assert.True(resultados[i].Puntuacion >= resultados[i + 1].Puntuacion);
        }
    }

    [Fact]
    public async Task RecomendarEspeculativoAsync_KMayorQueDisponibles_RetornaTodasDisponibles()
    {
        var corpus = CrearPeliculasPrueba();
        var usuario = CrearUsuarioPrueba();
        var motor = new MotorRecomendacion(corpus);

        int k = 10; // más grande que el número de candidatos
        var (resultados, _) = await motor.RecomendarEspeculativoAsync(usuario, k);

        // Debe devolver solo las películas no vistas
        int esperadas = corpus.Count - usuario.IdsVistos.Count;
        Assert.Equal(esperadas, resultados.Count);
    }
}
