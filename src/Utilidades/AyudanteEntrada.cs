namespace src.Utilidades;

public static class AyudanteEntrada
{
    public static int LeerInt(string mensaje, int min, int max)
    {
        while (true)
        {
            Console.Write($"{mensaje} ");
            var s = Console.ReadLine();
            if (int.TryParse(s, out var n) && n>=min && n<=max) return n;
            Console.WriteLine($"Ingrese un número entre {min} y {max}.");
        }
    }

    public static string LeerNoVacio(string mensaje)
    {
        while (true)
        {
            Console.Write(mensaje);
            var s = (Console.ReadLine() ?? string.Empty).Trim();
            if (!string.IsNullOrEmpty(s)) return s;
        }
    }
}