
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

        [Display(Prompt = "Material.Field.Name.Prompt", Description = "Material.Field.Name.Description", Name = "Material.Field.Name")]
        [Required(ErrorMessage = "Material.Field.Name.Required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Material.Field.Name.Length")]
        public string Nombre { get; set; }
        [Display(Prompt = "Material.Field.Description.Prompt", Description = "Material.Field.Description.Description", Name = "Material.Field.Description")]
        [Required(ErrorMessage = "Material.Field.Description.Required")]
        [StringLength(1000, MinimumLength = 20, ErrorMessage = "Material.Field.Description.Length")]
        public string Descripcion { get; set; }
        [Display(Name = "Material.Field.Status")]
        [Required(ErrorMessage = "Material.Field.Status.Required")]
        public EstadoMaterial Estado { get; set; }

        [Display(Name = "Material.Field.Category")]
        [Required(ErrorMessage = "Material.Field.Category.Required")]
        public CategoriaMaterial Categoria { get; set; }

        public string Imagen { get; set; }
        public IFormFile Fichero { get; set; }

        public string? NombreUsuarioAsignado { get; set; }
    }
}
