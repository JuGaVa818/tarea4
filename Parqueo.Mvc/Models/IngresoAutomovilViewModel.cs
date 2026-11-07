using System.Text.Json.Serialization;

namespace Parqueo.Mvc.Models;

public sealed class IngresoAutomovilViewModel
{
    [JsonPropertyName("id_parqueo")]
    public int IdParqueo { get; init; }

    [JsonPropertyName("id_automovil")]
    public int IdAutomovil { get; init; }

    [JsonPropertyName("fecha_hora_entrada")]
    public DateTime FechaHoraEntrada { get; init; }

    [JsonPropertyName("fecha_hora_salida")]
    public DateTime? FechaHoraSalida { get; init; }

    [JsonPropertyName("duracion_estadia_minutos")]
    public decimal? DuracionEstadiaMinutos { get; init; }

    [JsonPropertyName("duracion_estadia_horas")]
    public decimal? DuracionEstadiaHoras { get; init; }

    [JsonPropertyName("monto_total_pagar")]
    public decimal? MontoTotalPagar { get; init; }
}
