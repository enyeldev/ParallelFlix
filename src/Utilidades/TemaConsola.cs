namespace src.Utilidades;

public static class TemaConsola
{
    public static void Titulo(string texto)
    {
        Linea();
        Console.WriteLine(texto);
        Linea();
    }
    public static void Linea() => Console.WriteLine("────────────────────────────────────────────────────────────");

    public static void Etiqueta(string clave, string valor) => Console.WriteLine($"{clave}: {valor}");
}