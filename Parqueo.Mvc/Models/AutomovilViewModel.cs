using System.ComponentModel.DataAnnotations;

namespace Parqueo.Mvc.Models;

public sealed class AutomovilViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El color es obligatorio.")]
    [StringLength(40, ErrorMessage = "El color no puede superar 40 caracteres.")]
    public string Color { get; set; } = string.Empty;

    [Range(1886, 2100, ErrorMessage = "El año debe estar entre 1886 y 2100.")]
    public int Anio { get; set; }

    [Required(ErrorMessage = "El fabricante es obligatorio.")]
    [StringLength(80, ErrorMessage = "El fabricante no puede superar 80 caracteres.")]
    public string Fabricante { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecciona un tipo de automóvil.")]
    [RegularExpression("^(sedán|4x4|moto)$", ErrorMessage = "Selecciona un tipo válido.")]
    public string Tipo { get; set; } = string.Empty;
}
