
using System.ComponentModel.DataAnnotations;
using ApplicationCore.Domain.Enums;

namespace WebMarkerSpace.Models {
    public class PrestamoViewModel
    {
        [Display(Name = "Identificador Préstamo")]
        public long Id { get; set; }

        [Display(Name = "Usuario")]
        [Required(ErrorMessage = "El usuario es obligatorio.")]
        public long UsuarioId { get; set; }

        [Display(Name = "Nombre del Usuario")]
        public string? NombreUsuario { get; set; }

        [Display(Name = "Fecha de Creación")]
        [Required(ErrorMessage = "La fecha de creación es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Display(Name = "Estado")]
        [Required(ErrorMessage = "El estado del préstamo es obligatorio.")]
        public EstadoPrestamo Estado { get; set; }

        [Display(Name = "Total de Días")]
        [Required(ErrorMessage = "El total de días es obligatorio.")]
        [Range(1, 365, ErrorMessage = "El préstamo debe ser de entre 1 y 365 días.")]
        public int TotalDias { get; set; }

        public IList<LineaPrestamoViewModel> Lineas { get; set; } = new List<LineaPrestamoViewModel>();
    }
}
