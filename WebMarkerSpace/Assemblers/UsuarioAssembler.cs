
using ApplicationCore.Domain.EN;
using System;
using System.Collections.Generic;
using WebMarkerSpace.Models;

namespace WebMarkerSpace.Assemblers {
    public class UsuarioAssembler
    {
        public UsuarioViewModel ConvertirENToViewModel(Usuario en) {
            if (en == null)
                return null;

            UsuarioViewModel model = new UsuarioViewModel();
            model.Id = en.Id;
            model.Nombre = en.Nombre;
            model.Email = en.Email;
            model.Contrasenia = string.Empty;
            model.Rol = en.Rol;

            return model;
        }

        public IList<UsuarioViewModel> ConvertirListaENToViewModel(IList<Usuario> listaEN) {
            IList<UsuarioViewModel> listaModel = new List<UsuarioViewModel>();

            if (listaEN != null) {
                foreach (Usuario en in listaEN) {
                    UsuarioViewModel model = ConvertirENToViewModel(en);
                    if (model != null) {
                        listaModel.Add(model);
                    }
                }
            }

            return listaModel;
        }
    }
}
