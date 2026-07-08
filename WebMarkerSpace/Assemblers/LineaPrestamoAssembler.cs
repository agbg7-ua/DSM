
using ApplicationCore.Domain.EN;
using System.Collections.Generic;
using WebMarkerSpace.Models;

namespace WebMarkerSpace.Assemblers {
    public class LineaPrestamoAssembler
    {
        public LineaPrestamoViewModel ConvertirENToViewModel(LineaPrestamo en) {
            if (en == null)
                return null;

            LineaPrestamoViewModel model = new LineaPrestamoViewModel();
            model.Id = en.Id;
            model.PrestamoId = en.PrestamoId;
            model.MaterialId = en.MaterialId;

            if (en.Material != null) {
                model.NombreMaterial = en.Material.Nombre;
                model.DescripcionMaterial = en.Material.Descripcion;
            }
            model.DiasEstimados = en.DiasEstimados;

            return model;
        }

        public IList<LineaPrestamoViewModel> ConvertirListaENToViewModel(IList<LineaPrestamo> listaEN) {
            IList<LineaPrestamoViewModel> listaModel = new List<LineaPrestamoViewModel>();
            if (listaEN != null) {
                foreach (LineaPrestamo en in listaEN) {
                    listaModel.Add(ConvertirENToViewModel(en));
                }
            }
            return listaModel;
        }
    }
}
