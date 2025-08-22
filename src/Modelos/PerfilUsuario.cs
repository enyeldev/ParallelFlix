using System;

namespace src.Modelos;

public class PerfilUsuario
{
    public string NombreUsuario { get; }
    public HashSet<string> GenerosPreferidos { get; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> EtiquetasPreferidas { get; } = new(StringComparer.OrdinalIgnoreCase);
    public List<int> IdsVistos { get; } = new();
    public HashSet<int> IdsMiLista { get; } = new();

    public PerfilUsuario(string nombreUsuario)
    {
        NombreUsuario = nombreUsuario;
    }
}
