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
            Console.Write("⌨  Seleccione #, o teclee M/B/S o filtros G/E/D/V: ");
            var key = Console.ReadLine()?.Trim();
            if (string.Equals(key, "S", StringComparison.OrdinalIgnoreCase)) break;
            if (string.Equals(key, "M", StringComparison.OrdinalIgnoreCase)) { await MiListaAsync(); continue; }
            if (string.Equals(key, "B", StringComparison.OrdinalIgnoreCase)) { await BuscarAsync(); continue; }
            
            // Opciones de filtrado
            if (string.Equals(key, "G", StringComparison.OrdinalIgnoreCase)) { await FiltrarPorGeneroAsync(); continue; }
            if (string.Equals(key, "E", StringComparison.OrdinalIgnoreCase)) { await FiltrarPorEtiquetasAsync(); continue; }
            if (string.Equals(key, "D", StringComparison.OrdinalIgnoreCase)) { await FiltrarPorDuracionAsync(); continue; }
            if (string.Equals(key, "V", StringComparison.OrdinalIgnoreCase)) { await MostrarMasVistosAsync(); continue; }

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
        Console.WriteLine("Filtrar por:  [G] Género   [E] Etiquetas   [D] Duración   [V] Más Vistos");
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

    // Métodos de filtrado
    private async Task FiltrarPorGeneroAsync()
    {
        Console.Clear();
        TemaConsola.Titulo("🎭 Filtrar por Género");
        
        // Mostrar géneros disponibles
        var generos = _peliculas.Select(p => p.Genero).Distinct().OrderBy(g => g).ToList();
        TemaConsola.Linea();
        Console.WriteLine("Géneros disponibles:");
        for (int i = 0; i < generos.Count; i++)
        {
            Console.WriteLine($"[{i + 1}] {generos[i]}");
        }
        Console.WriteLine("[0] Escribir género personalizado");
        TemaConsola.Linea();
        
        var opcion = AyudanteEntrada.LeerInt("Seleccione género (0 para personalizado):", 0, generos.Count);
        string generoSeleccionado;
        
        if (opcion == 0)
        {
            generoSeleccionado = AyudanteEntrada.LeerNoVacio("Escriba el género: ");
        }
        else
        {
            generoSeleccionado = generos[opcion - 1];
        }
        
        var peliculasFiltradas = _peliculas
            .Where(p => p.Genero.Contains(generoSeleccionado, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(p => p.Calificacion)
            .Take(20)
            .ToList();
            
        await MostrarSelectorAsync(peliculasFiltradas);
    }

    private async Task FiltrarPorEtiquetasAsync()
    {
        Console.Clear();
        TemaConsola.Titulo("📝 Filtrar por Etiquetas");
        
        // Mostrar etiquetas más comunes
        var todasEtiquetas = _peliculas.SelectMany(p => p.Etiquetas).ToList();
        var etiquetasFrecuentes = todasEtiquetas
            .GroupBy(e => e, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(g => g.Count())
            .Take(15)
            .Select(g => g.Key)
            .ToList();
            
        TemaConsola.Linea();
        Console.WriteLine("Etiquetas más frecuentes:");
        for (int i = 0; i < etiquetasFrecuentes.Count; i++)
        {
            var cantidad = todasEtiquetas.Count(e => e.Equals(etiquetasFrecuentes[i], StringComparison.OrdinalIgnoreCase));
            Console.WriteLine($"[{i + 1}] {etiquetasFrecuentes[i]} ({cantidad} películas)");
        }
        Console.WriteLine("[0] Escribir etiqueta personalizada");
        TemaConsola.Linea();
        
        var opcion = AyudanteEntrada.LeerInt("Seleccione etiqueta (0 para personalizada):", 0, etiquetasFrecuentes.Count);
        string etiquetaSeleccionada;
        
        if (opcion == 0)
        {
            etiquetaSeleccionada = AyudanteEntrada.LeerNoVacio("Escriba la etiqueta: ");
        }
        else
        {
            etiquetaSeleccionada = etiquetasFrecuentes[opcion - 1];
        }
        
        var peliculasFiltradas = _peliculas
            .Where(p => p.Etiquetas.Any(e => e.Contains(etiquetaSeleccionada, StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(p => p.Calificacion)
            .Take(20)
            .ToList();
            
        await MostrarSelectorAsync(peliculasFiltradas);
    }

    private async Task FiltrarPorDuracionAsync()
    {
        Console.Clear();
        TemaConsola.Titulo("⏱ Filtrar por Duración");
        
        TemaConsola.Linea();
        Console.WriteLine("Seleccione rango de duración:");
        Console.WriteLine("[1] Cortas (menos de 90 min)");
        Console.WriteLine("[2] Normales (90-120 min)");
        Console.WriteLine("[3] Largas (120-150 min)");
        Console.WriteLine("[4] Muy largas (más de 150 min)");
        Console.WriteLine("[5] Personalizado");
        TemaConsola.Linea();
        
        var opcion = AyudanteEntrada.LeerInt("Seleccione opción:", 1, 5);
        List<Pelicula> peliculasFiltradas = new();
        
        switch (opcion)
        {
            case 1: // Cortas
                peliculasFiltradas = _peliculas
                    .Where(p => ExtraerMinutos(p.Duracion) < 90)
                    .OrderByDescending(p => p.Calificacion)
                    .Take(20)
                    .ToList();
                break;
            case 2: // Normales
                peliculasFiltradas = _peliculas
                    .Where(p => ExtraerMinutos(p.Duracion) >= 90 && ExtraerMinutos(p.Duracion) <= 120)
                    .OrderByDescending(p => p.Calificacion)
                    .Take(20)
                    .ToList();
                break;
            case 3: // Largas
                peliculasFiltradas = _peliculas
                    .Where(p => ExtraerMinutos(p.Duracion) > 120 && ExtraerMinutos(p.Duracion) <= 150)
                    .OrderByDescending(p => p.Calificacion)
                    .Take(20)
                    .ToList();
                break;
            case 4: // Muy largas
                peliculasFiltradas = _peliculas
                    .Where(p => ExtraerMinutos(p.Duracion) > 150)
                    .OrderByDescending(p => p.Calificacion)
                    .Take(20)
                    .ToList();
                break;
            case 5: // Personalizado
                var minimo = AyudanteEntrada.LeerInt("Duración mínima (minutos):", 0, 300);
                var maximo = AyudanteEntrada.LeerInt("Duración máxima (minutos):", minimo, 400);
                peliculasFiltradas = _peliculas
                    .Where(p => ExtraerMinutos(p.Duracion) >= minimo && ExtraerMinutos(p.Duracion) <= maximo)
                    .OrderByDescending(p => p.Calificacion)
                    .Take(20)
                    .ToList();
                break;
        }
        
        await MostrarSelectorAsync(peliculasFiltradas);
    }

    private async Task MostrarMasVistosAsync()
    {
        Console.Clear();
        TemaConsola.Titulo("⭐ Películas Más Populares");
        
        TemaConsola.Linea();
        Console.WriteLine("Ordenar por:");
        Console.WriteLine("[1] Mejor calificación");
        Console.WriteLine("[2] Más recientes");
        Console.WriteLine("[3] Clásicos (anteriores a 2000)");
        Console.WriteLine("[4] Éxitos de la década (2010-2020)");
        TemaConsola.Linea();
        
        var opcion = AyudanteEntrada.LeerInt("Seleccione criterio:", 1, 4);
        List<Pelicula> peliculasFiltradas = new();
        
        switch (opcion)
        {
            case 1: // Mejor calificación
                peliculasFiltradas = _peliculas
                    .OrderByDescending(p => p.Calificacion)
                    .ThenByDescending(p => p.Ano)
                    .Take(20)
                    .ToList();
                break;
            case 2: // Más recientes
                peliculasFiltradas = _peliculas
                    .Where(p => p.Ano >= 2020)
                    .OrderByDescending(p => p.Ano)
                    .ThenByDescending(p => p.Calificacion)
                    .Take(20)
                    .ToList();
                break;
            case 3: // Clásicos
                peliculasFiltradas = _peliculas
                    .Where(p => p.Ano < 2000)
                    .OrderByDescending(p => p.Calificacion)
                    .Take(20)
                    .ToList();
                break;
            case 4: // Éxitos de la década
                peliculasFiltradas = _peliculas
                    .Where(p => p.Ano >= 2010 && p.Ano <= 2020)
                    .OrderByDescending(p => p.Calificacion)
                    .Take(20)
                    .ToList();
                break;
        }
        
        await MostrarSelectorAsync(peliculasFiltradas);
    }

    // Método auxiliar para extraer minutos de una cadena de duración
    private int ExtraerMinutos(string duracion)
    {
        if (string.IsNullOrEmpty(duracion)) return 0;
        
        // Buscar números en la cadena (ej: "120 min", "2h 30m", "90")
        var numerosEncontrados = System.Text.RegularExpressions.Regex.Matches(duracion, @"\d+")
            .Cast<System.Text.RegularExpressions.Match>()
            .Select(m => int.Parse(m.Value))
            .ToList();
            
        if (!numerosEncontrados.Any()) return 90; // Valor por defecto
        
        // Si contiene "h" probablemente son horas y minutos
        if (duracion.Contains("h") && numerosEncontrados.Count >= 2)
        {
            return numerosEncontrados[0] * 60 + numerosEncontrados[1];
        }
        
        // Si es un solo número, asumimos que son minutos
        return numerosEncontrados[0];
    }
}
