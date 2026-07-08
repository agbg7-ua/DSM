
using WebMarkerSpace.Models;
using ApplicationCore.Domain.EN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebMarkerSpace.Models;

namespace WebMarkerSpace.Assemblers {
    public class MaterialAssembler
    {
        public MaterialViewModel ConvertirENToViewModel(Material en)
        {
            MaterialViewModel mat = new MaterialViewModel();
            mat.Id = (int)en.Id;
            mat.Nombre = en.Nombre;
            mat.Descripcion = en.Descripcion;
            mat.Estado = en.Estado;
            mat.Categoria = en.Categoria;
            mat.Imagen = en.Imagen;
            mat.NombreUsuarioAsignado = en.UsuarioAsignado?.Nombre;

            return mat;
        }
        public IList<MaterialViewModel> ConvertirListaENToViewModel(IList<Material> ens)
        {
            IList<MaterialViewModel> mats = new List<MaterialViewModel>();
            foreach (Material en in ens)
            {
                mats.Add(ConvertirENToViewModel(en));
            }
            return mats;
        }
    }
}
