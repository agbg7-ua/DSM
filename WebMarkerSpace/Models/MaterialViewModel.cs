// "Copyright (c) YOUR_COMPANY. All rights reserved."
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ApplicationCore.Domain.Enums;

namespace WebMarkerSpace.Models {
    public class MaterialViewModel
    {
        [ScaffoldColumn(false)]
        public int Id { get; set; }

        [Display (Prompt = "Escribe el nombre del material", Description = "Nombre del material", Name = "Nombre")]
        [Required(ErrorMessage = "Debe indicar un nombre para el material")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre del material debe tener entre 3 y 100 caracteres")]
        public string Nombre { get; set; }
        [Display(Prompt = "Describe el material", Description = "Descripción del material", Name = "Descripción")]
        [Required(ErrorMessage = "Debe indicar una descripción para el material")]
        [StringLength(1000, MinimumLength = 20, ErrorMessage = "El nombre del material debe tener entre 20 y 1000 caracteres")]
        public string Descripcion { get; set; }
        [Display(Name = "Estado del Material")]
        [Required(ErrorMessage = "Debe seleccionar un estado para el material")]
        public EstadoMaterial Estado { get; set; }

        [Display(Name = "Categoría")]
        [Required(ErrorMessage = "Debe seleccionar una categoría para el material")]
        public CategoriaMaterial Categoria { get; set; }

        public string Imagen { get; set; }
        public IFormFile Fichero { get; set; }

        // Solo lectura: quién tiene el material actualmente prestado (si alguien).
        // No es un campo redundante con Estado: Estado describe la condición
        // física (disponible/en mantenimiento/roto/prestado); esto dice A QUIÉN.
        public string? NombreUsuarioAsignado { get; set; }
    }
}
