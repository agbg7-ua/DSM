// "Copyright (c) YOUR_COMPANY. All rights reserved."

using System.ComponentModel.DataAnnotations;

namespace WebMarkerSpace.Models {
    public class LineaPrestamoViewModel
    {
        public long Id { get; set; }

        [Required]
        public long PrestamoId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un material.")]
        [Display(Name = "Material")]
        public long MaterialId { get; set; }

        // Añadimos esta propiedad para que coincida con el dominio
        [Required(ErrorMessage = "Los días estimados son obligatorios.")]
        [Range(1, 365, ErrorMessage = "El periodo debe ser entre 1 y 365 días.")]
        [Display(Name = "Días Estimados")]
        public int DiasEstimados { get; set; }

        // Propiedades de lectura para mostrar en la tabla (no se editan)
        public string? NombreMaterial { get; set; }
        public string? DescripcionMaterial { get; set; }

    }
}
