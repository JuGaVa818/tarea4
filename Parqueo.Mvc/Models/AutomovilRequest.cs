namespace Parqueo.Mvc.Models;

public sealed class AutomovilRequest
{
    public string Color { get; init; } = string.Empty;
    public int Anio { get; init; }
    public string Fabricante { get; init; } = string.Empty;
    public string Tipo { get; init; } = string.Empty;
}
