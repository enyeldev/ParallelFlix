using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using src.Modelos;


namespace src.Servicios
{
    public sealed class SesionUsuario
    {
        public PerfilUsuario Perfil { get; }
        public List<int> Vistos { get; } = new();

        public SesionUsuario(string nombreUsuario)
        {
            Perfil = new PerfilUsuario(nombreUsuario);
        }
    }
}