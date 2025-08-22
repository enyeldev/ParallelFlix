using Xunit;
using src.Modelos;
using src.Servicios;

public class SesionUsuarioTests
{
    [Fact]
    public void Constructor_CreaPerfilConNombreUsuario()
    {
        var sesion = new SesionUsuario("usuario1");

        Assert.NotNull(sesion.Perfil);
        Assert.Equal("usuario1", sesion.Perfil.NombreUsuario);
    }

    [Fact]
    public void Vistos_EsListaVaciaAlInicio()
    {
        var sesion = new SesionUsuario("usuario1");

        Assert.NotNull(sesion.Vistos);
        Assert.Empty(sesion.Vistos);
    }

    [Fact]
    public void AgregarVisto_AgregaCorrectamente()
    {
        var sesion = new SesionUsuario("usuario1");

        sesion.Vistos.Add(5);
        sesion.Vistos.Add(10);

        Assert.Equal(2, sesion.Vistos.Count);
        Assert.Contains(5, sesion.Vistos);
        Assert.Contains(10, sesion.Vistos);
    }
}
