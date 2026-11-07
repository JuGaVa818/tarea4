using Microsoft.AspNetCore.Mvc;
using Parqueo.Mvc.Models;
using Parqueo.Mvc.Services;

namespace Parqueo.Mvc.Controllers;

public sealed class AutomovilesController(AutomovilesApiClient apiClient) : Controller
{
    private static readonly HashSet<string> TiposAutomovil =
        new(["sedán", "4x4", "moto"], StringComparer.Ordinal);

    public async Task<IActionResult> Index(
        int? anioDesde,
        int? anioHasta,
        CancellationToken cancellationToken)
    {
        if (anioDesde is < 1886 or > 2100)
        {
            ModelState.AddModelError(nameof(anioDesde), "El año debe estar entre 1886 y 2100.");
        }

        if (anioHasta is < 1886 or > 2100)
        {
            ModelState.AddModelError(nameof(anioHasta), "El año debe estar entre 1886 y 2100.");
        }

        if (anioDesde > anioHasta)
        {
            ModelState.AddModelError(
                string.Empty,
                "El año inicial debe ser menor o igual que el año final.");
        }

        var automoviles = ModelState.IsValid
            ? await apiClient.ObtenerTodosAsync(anioDesde, anioHasta, cancellationToken)
            : [];
        return View(new AutomovilesIndexViewModel
        {
            AnioDesde = anioDesde,
            AnioHasta = anioHasta,
            Automoviles = automoviles
        });
    }

    public IActionResult MaestroDetalle() => View();

    [HttpGet]
    public async Task<IActionResult> ConsultarPorTipo(
        string? tipo,
        CancellationToken cancellationToken)
    {
        if (tipo is null || !TiposAutomovil.Contains(tipo))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "El tipo de automóvil no es válido."
            });
        }

        return Json(await apiClient.ObtenerPorTipoAsync(tipo, cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> IngresosPorAutomovil(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "El ID del automóvil debe ser positivo."
            });
        }

        if (await apiClient.ObtenerPorIdAsync(id, cancellationToken) is null)
        {
            return NotFound();
        }

        var ingresos = await apiClient.ObtenerIngresosAsync(cancellationToken);
        return Json(ingresos.Where(ingreso => ingreso.IdAutomovil == id).ToArray());
    }

    public IActionResult Create() => View(new AutomovilViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        AutomovilViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        using var response = await apiClient.CrearAsync(ToRequest(model), cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            TempData["Mensaje"] = "El automóvil se creó correctamente.";
            return RedirectToAction(nameof(Index));
        }

        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            await AutomovilesApiClient.AgregarErroresAsync(
                response, ModelState, cancellationToken);
            return View(model);
        }

        return StatusCode((int)response.StatusCode);
    }

    public async Task<IActionResult> Details(
        int id,
        CancellationToken cancellationToken)
    {
        var automovil = await apiClient.ObtenerPorIdAsync(id, cancellationToken);
        return automovil is null ? NotFound() : View(automovil);
    }

    public async Task<IActionResult> Edit(
        int id,
        CancellationToken cancellationToken)
    {
        var automovil = await apiClient.ObtenerPorIdAsync(id, cancellationToken);
        return automovil is null ? NotFound() : View(automovil);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        AutomovilViewModel model,
        CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        using var response = await apiClient.ActualizarAsync(
            id, ToRequest(model), cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            TempData["Mensaje"] = "El automóvil se actualizó correctamente.";
            return RedirectToAction(nameof(Index));
        }

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound();
        }

        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            await AutomovilesApiClient.AgregarErroresAsync(
                response, ModelState, cancellationToken);
            return View(model);
        }

        return StatusCode((int)response.StatusCode);
    }

    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var automovil = await apiClient.ObtenerPorIdAsync(id, cancellationToken);
        return automovil is null ? NotFound() : View(automovil);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(
        int id,
        CancellationToken cancellationToken)
    {
        using var response = await apiClient.EliminarAsync(id, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            TempData["Mensaje"] = "El automóvil se eliminó correctamente.";
            return RedirectToAction(nameof(Index));
        }

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound();
        }

        if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            var automovil = await apiClient.ObtenerPorIdAsync(id, cancellationToken);
            if (automovil is null)
            {
                return NotFound();
            }

            ModelState.AddModelError(
                string.Empty,
                await AutomovilesApiClient.ObtenerMensajeErrorAsync(
                    response, cancellationToken));
            Response.StatusCode = StatusCodes.Status409Conflict;
            return View(automovil);
        }

        return StatusCode((int)response.StatusCode);
    }

    private static AutomovilRequest ToRequest(AutomovilViewModel model) =>
        new()
        {
            Color = model.Color.Trim(),
            Anio = model.Anio,
            Fabricante = model.Fabricante.Trim(),
            Tipo = model.Tipo
        };
}
