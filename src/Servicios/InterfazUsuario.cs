using src.Modelos;
using src.Utilidades;
using src.Servicios;
using src.Servicios.AlgoritmoEspeculativo;

namespace src.Servicios;

public sealed class InterfazUsuario
{
    private readonly List<Pelicula> _peliculas;
    private readonly MotorRecomendacion _motor;
    private readonly SesionUsuario _sesion;
    private readonly Random _rnd = new();

    public InterfazUsuario(List<Pelicula> peliculas, MotorRecomendacion motor, SesionUsuario sesion)
    {
        _peliculas = peliculas;
        _motor = motor;
        _sesion = sesion;
    }

    public async Task EjecutarAsync()
    {
        await _motor.CompararyMedirRendimientoAsync(_sesion.Perfil, 10);
        _motor.MetricasRendimiento.FinalizarMetricas(10);
        
        while (true)
        {
            Console.Clear();
            TemaConsola.Titulo("🎬  Sistema de Recomendación Paralelo - NETCLI 1.0");
            TemaConsola.Etiqueta("Usuario", $"[{_sesion.Perfil.NombreUsuario}]");
            TemaConsola.Etiqueta("Modo", "🌙 Oscuro");
            TemaConsola.Linea();

            Console.WriteLine("[M] Mi Lista  [B] Buscar  [S] Salir");

            var (recs, tiempos) = await _motor.RecomendarEspeculativoAsync(_sesion.Perfil, 10);
            ImprimirRecomendaciones(recs);

            TemaConsola.Linea();
            Console.Write("⌨  Seleccione #, o teclee M/B/S: ");
            var key = Console.ReadLine()?.Trim();
            if (string.Equals(key, "S", StringComparison.OrdinalIgnoreCase)) break;
            if (string.Equals(key, "M", StringComparison.OrdinalIgnoreCase)) { await MiListaAsync(); continue; }
            if (string.Equals(key, "B", StringComparison.OrdinalIgnoreCase)) { await BuscarAsync(); continue; }

            if (int.TryParse(key, out var indice) && indice>=1 && indice<=recs.Count)
            {
                await DetallesAsync(recs[indice-1].Pelicula);
            }
        }

        Console.Clear();
        TemaConsola.Titulo("📊 Metricas de Rendimiento del Sistema");
        TemaConsola.Linea();
        _motor.MetricasRendimiento.MostrarMetricasParalelismo();
        Console.WriteLine("👋 Sesion finalizada. (Los datos de la sesion no se guardan)");
        Console.WriteLine("\nPresione cualquier tecla para continuar...");
        Console.ReadKey();
    }

    private void ImprimirRecomendaciones(List<Recomendacion> recs)
    {
        TemaConsola.Linea();
        Console.WriteLine("Recomendaciones para ti:");
        TemaConsola.Linea();
        for (int i=0;i<recs.Count;i++)
        {
            var r = recs[i];
            Console.WriteLine($"[{i+1}]  [███▌] {r.Pelicula.Titulo}   🎭 Género: {r.Pelicula.Genero} Etiquetas: {string.Join(", ", r.Pelicula.Etiquetas)} ⭐ {r.Pelicula.Calificacion:F1} | ⏱ {r.Pelicula.Duracion} ");
        }
        TemaConsola.Linea();
        Console.WriteLine("Filtrar por:  [1] Género   [2] Etiquetas   [3] Duración   [4] Más Vistos");
    }

    private async Task DetallesAsync(Pelicula p)
    {
        while (true)
        {
            Console.Clear();
            TemaConsola.Titulo($"📽  Detalles de la Recomendación - {p.Titulo}");
            Console.WriteLine("[███▌] Portada simulada");
            Console.WriteLine($"🎭 Género: {p.Genero}📝 Etiquetas: {string.Join(", ", p.Etiquetas)} ⭐ Calificación: {p.Calificacion:F1} / 5.0 ⏱ Duración: {p.Duracion} 📅 Año: {p.Ano} 👤 Director: {p.Director} ");
            TemaConsola.Linea();
            Console.WriteLine("[ R ] Regresar a lista      [ P ] Reproducir       [ A ] Agregar a mi lista");
            TemaConsola.Linea();
            Console.Write("⌨  Ingrese opción: ");
            var opt = Console.ReadLine()?.Trim().ToUpperInvariant();
            if (opt == "R") return; // volver a inicio
            if (opt == "A") { _sesion.Perfil.IdsMiLista.Add(p.Id); Console.WriteLine("✅ Agregado a Mi Lista."); Thread.Sleep(600); continue; }
            if (opt == "P")
            {
                SimuladorReproduccion.ReproducirPorDiezSegundos(p.Titulo);
                if (!_sesion.Perfil.IdsVistos.Contains(p.Id)) _sesion.Perfil.IdsVistos.Add(p.Id);
                _sesion.Vistos.Add(p.Id);
                // Recomendaciones similares tras ver
                await MostrarDespuesDeVerAsync(p);
                return; // luego regresamos a inicio
            }
        }
    }

    private async Task MostrarDespuesDeVerAsync(Pelicula visto)
    {
        var similares = _peliculas.Where(x => x.Id != visto.Id && (x.Genero.Equals(visto.Genero, StringComparison.OrdinalIgnoreCase) || x.Etiquetas.Intersect(visto.Etiquetas, StringComparer.OrdinalIgnoreCase).Any()))
                          .OrderByDescending(x => x.Calificacion)
                          .Take(25)
                          .ToList();
        var elecciones = similares.OrderBy(_ => _rnd.Next()).Take(10).Select(x => new Recomendacion{ Pelicula = x, Puntuacion = x.Calificacion }).ToList();

        TemaConsola.Linea();
        Console.WriteLine("🎯 Basado en lo que viste, también te podría gustar:");
        TemaConsola.Linea();
        for (int i=0;i<elecciones.Count;i++)
        {
            var r = elecciones[i];
            Console.WriteLine($"[{i+1}] {r.Pelicula.Titulo}     🎭 {r.Pelicula.Genero}");
        }
        TemaConsola.Linea();
        Console.Write("Seleccione # para ver detalles o presione ENTER para regresar: ");
        var s = Console.ReadLine();
        if (int.TryParse(s, out var n) && n>=1 && n<=elecciones.Count)
        {
            await DetallesAsync(elecciones[n-1].Pelicula);
        }
    }

    private async Task MiListaAsync()
    {
        while (true)
        {
            Console.Clear();
            TemaConsola.Titulo("📚 Mi Lista");
            var lista = _peliculas.Where(m => _sesion.Perfil.IdsMiLista.Contains(m.Id)).ToList();
            if (lista.Count == 0) { Console.WriteLine("(Vacía)[R] Regresar"); if ((Console.ReadLine() ?? "").Trim().ToUpperInvariant()=="R") return; continue; }

            for(int i=0;i<lista.Count;i++)
                Console.WriteLine($"[{i+1}] {lista[i].Titulo}  🎭 {lista[i].Genero}  ⭐ {lista[i].Calificacion:F1}");
            Console.WriteLine("[D] Eliminar por #   [V] Ver detalles   [R] Regresar");
            Console.Write("Opción: ");
            var op = (Console.ReadLine() ?? string.Empty).Trim().ToUpperInvariant();
            if (op == "R") return;
            if (op.StartsWith("D"))
            {
                var idx = AyudanteEntrada.LeerInt("# a eliminar:", 1, lista.Count) - 1;
                _sesion.Perfil.IdsMiLista.Remove(lista[idx].Id);
            }
            else if (op.StartsWith("V"))
            {
                var idx = AyudanteEntrada.LeerInt("# a ver:", 1, lista.Count) - 1;
                await DetallesAsync(lista[idx]);
            }
        }
    }

    private async Task BuscarAsync()
    {
        Console.Clear();
        TemaConsola.Titulo("🔎 Búsqueda");
        Console.WriteLine("[1] Por género   [2] Por título   [R] Regresar");
        Console.Write("Opción: ");
        var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
        if (s == "R") return;
        if (s == "1")
        {
            var g = AyudanteEntrada.LeerNoVacio("Género: ");
            var items = _peliculas.Where(m => m.Genero.Contains(g, StringComparison.OrdinalIgnoreCase)).OrderByDescending(m=>m.Calificacion).Take(20).ToList();
            await MostrarSelectorAsync(items);
        }
        else if (s == "2")
        {
            var t = AyudanteEntrada.LeerNoVacio("Título contiene: ");
            var items = _peliculas.Where(m => m.Titulo.Contains(t, StringComparison.OrdinalIgnoreCase)).Take(20).ToList();
            await MostrarSelectorAsync(items);
        }
    }

    private async Task MostrarSelectorAsync(List<Pelicula> items)
    {
        if (items.Count == 0) { Console.WriteLine("Sin resultados. ENTER para continuar..."); Console.ReadLine(); return; }
        TemaConsola.Linea();
        for (int i=0;i<items.Count;i++) Console.WriteLine($"[{i+1}] {items[i].Titulo}  🎭 {items[i].Genero}  ⭐ {items[i].Calificacion:F1}");
        TemaConsola.Linea();
        var n = AyudanteEntrada.LeerInt("Seleccione # (0 para cancelar):", 0, items.Count);
        if (n==0) return;
        await DetallesAsync(items[n-1]);
    }
}