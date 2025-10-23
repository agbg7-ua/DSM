using ApplicationCore.Domain.EN;

namespace ApplicationCore.Domain.Repositories
{
    public interface IUsuarioRepository : IRepository<Usuario, long>
    {
        Usuario GetByCorreo(string correo);
    }
}