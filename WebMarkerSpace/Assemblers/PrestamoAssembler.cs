
using ApplicationCore.Domain.EN;
using WebMarkerSpace.Models;

namespace WebMarkerSpace.Assemblers {
    public class PrestamoAssembler
    {
        public PrestamoViewModel ConvertirENToViewModel(Prestamo en) {
            if (en == null)
                return null;

            PrestamoViewModel model = new PrestamoViewModel();
            model.Id = en.Id;
            model.UsuarioId = en.UsuarioId;
            model.FechaCreacion = en.FechaCreacion;
            model.Estado = en.Estado;
            model.TotalDias = en.TotalDias;

            if (en.Usuario != null) {
                model.NombreUsuario = en.Usuario.Nombre;
                if (model.UsuarioId == 0) {
                    model.UsuarioId = en.Usuario.Id;
                }
            }
            model.Lineas = new LineaPrestamoAssembler().ConvertirListaENToViewModel(en.LineasPrestamo.ToList());
            return model;
        }

        public IList<PrestamoViewModel> ConvertirListaENToViewModel(IList<Prestamo> listaEN) {
            IList<PrestamoViewModel> listaModel = new List<PrestamoViewModel>();
            if (listaEN != null) {
                foreach (Prestamo en in listaEN) {
                    listaModel.Add(ConvertirENToViewModel(en));
                }
            }
            return listaModel;
        }
    }
}
