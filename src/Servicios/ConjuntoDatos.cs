using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using src.Modelos;
using System.Text.Json;

namespace src.Servicios
{
    public static class ConjuntoDatos
    {
        public static List<Pelicula> CargarPeliculas(string ruta)
        {
            using var fs = File.OpenRead(ruta);
            var peliculas = JsonSerializer.Deserialize<List<Pelicula>>(fs, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return peliculas ?? new List<Pelicula>();
        }
    }
}
