using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    // Ejemplo minimal de CEN para Usuario
    public class UsuarioCEN
    {
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IUnitOfWork _uow;

        public UsuarioCEN(IUsuarioRepository usuarioRepo, IUnitOfWork uow)
        {
            _usuarioRepo = usuarioRepo;
            _uow = uow;
        }

        public void Crear(string nombre, string correo, string contrasena, string? direccion = null)
        {
            var u = new Usuario
            {
                Nombre = nombre,
                Correo = correo,
                Contrasena = contrasena,
                Direccion = direccion
            };

            _usuarioRepo.New(u);
            _uow.SaveChanges();
        }
    }
}