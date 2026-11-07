using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Parqueo.Mvc.Models;

namespace Parqueo.Mvc.Services;

public sealed class AutomovilesApiClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<AutomovilViewModel>> ObtenerTodosAsync(
        int? anioDesde,
        int? anioHasta,
        CancellationToken cancellationToken)
    {
        var query = new Dictionary<string, string?>();
        if (anioDesde.HasValue)
        {
            query["anioDesde"] = anioDesde.Value.ToString();
        }

        if (anioHasta.HasValue)
        {
            query["anioHasta"] = anioHasta.Value.ToString();
        }

        var uri = query.Count == 0
            ? "api/automoviles"
            : QueryHelpers.AddQueryString("api/automoviles", query);
        using var response = await httpClient.GetAsync(uri, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<AutomovilViewModel>>(
                   cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException(
                   "La API devolvió una lista de automóviles vacía o inválida.");
    }

    public async Task<AutomovilViewModel?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(
            $"api/automoviles/{id}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AutomovilViewModel>(
                   cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException(
                   "La API devolvió un automóvil vacío o inválido.");
    }

    public async Task<IReadOnlyList<AutomovilViewModel>> ObtenerPorTipoAsync(
        string tipo,
        CancellationToken cancellationToken)
    {
        var uri = QueryHelpers.AddQueryString(
            "api/automoviles",
            new Dictionary<string, string?> { ["tipo"] = tipo });
        using var response = await httpClient.GetAsync(uri, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<AutomovilViewModel>>(
                   cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException(
                   "La API devolvió una lista de automóviles vacía o inválida.");
    }

    public async Task<IReadOnlyList<IngresoAutomovilViewModel>> ObtenerIngresosAsync(
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(
            "api/ingresos", cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<IngresoAutomovilViewModel>>(
                   cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException(
                   "La API devolvió una lista de ingresos vacía o inválida.");
    }

    public Task<HttpResponseMessage> CrearAsync(
        AutomovilRequest request,
        CancellationToken cancellationToken) =>
        httpClient.PostAsJsonAsync("api/automoviles", request, cancellationToken);

    public Task<HttpResponseMessage> ActualizarAsync(
        int id,
        AutomovilRequest request,
        CancellationToken cancellationToken) =>
        httpClient.PutAsJsonAsync($"api/automoviles/{id}", request, cancellationToken);

    public Task<HttpResponseMessage> EliminarAsync(
        int id,
        CancellationToken cancellationToken) =>
        httpClient.DeleteAsync($"api/automoviles/{id}", cancellationToken);

    public static async Task AgregarErroresAsync(
        HttpResponseMessage response,
        ModelStateDictionary modelState,
        CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        if (root.TryGetProperty("errors", out var errors) &&
            errors.ValueKind == JsonValueKind.Object)
        {
            foreach (var field in errors.EnumerateObject())
            {
                foreach (var error in field.Value.EnumerateArray())
                {
                    modelState.AddModelError(field.Name, error.GetString() ?? string.Empty);
                }
            }

            return;
        }

        var message = root.TryGetProperty("detail", out var detail)
            ? detail.GetString()
            : root.TryGetProperty("title", out var title)
                ? title.GetString()
                : null;
        modelState.AddModelError(
            string.Empty,
            message ?? $"La API rechazó la solicitud ({(int)response.StatusCode}).");
    }

    public static async Task<string> ObtenerMensajeErrorAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        if (root.TryGetProperty("detail", out var detail) &&
            !string.IsNullOrWhiteSpace(detail.GetString()))
        {
            return detail.GetString()!;
        }

        if (root.TryGetProperty("title", out var title) &&
            !string.IsNullOrWhiteSpace(title.GetString()))
        {
            return title.GetString()!;
        }

        return $"La API rechazó la solicitud ({(int)response.StatusCode}).";
    }
}
