using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace src.Servicios

    public static class SimuladorReproduccion
    {
        public static void ReproducirPorDiezSegundos(string titulo)
        {
            Console.WriteLine("
    Reproduciendo: " + titulo);
            var inicio = DateTime.UtcNow;
            var fin = inicio.AddSeconds(10);
            while (DateTime.UtcNow < fin)
            {
                RenderizarProgreso(1 - (fin - DateTime.UtcNow).TotalSeconds/10.0);
                Thread.Sleep(250);
            }
            RenderizarProgreso(1.0);
            Console.WriteLine("
    ✅ Reproducción finalizada.");
        }

        private static void RenderizarProgreso(double p)
        {
            p = Math.Clamp(p, 0, 1);
            int ancho = 30;
            int llenado = (int)(p * ancho);
            Console.Write("
    [" + new string('█', llenado) + new string(' ', ancho - llenado) + $"] {p:P0}");
        }
    }