namespace Parqueo.Mvc.Models;

public sealed class AutomovilesIndexViewModel
{
    public int? AnioDesde { get; init; }
    public int? AnioHasta { get; init; }
    public IReadOnlyList<AutomovilViewModel> Automoviles { get; init; } = [];
}
