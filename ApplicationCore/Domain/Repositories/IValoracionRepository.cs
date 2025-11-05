using ApplicationCore.Domain.EN;
using System.Collections.Generic;

namespace ApplicationCore.Domain.Repositories
{
    public interface IValoracionRepository : IRepository<Valoracion, long>
    {
        IEnumerable<Valoracion> GetByProducto(long productoId);
        bool ExisteValoracion(long usuarioId, long productoId);
        IEnumerable<Valoracion> GetByUsuario(long usuarioId);
        void Delete(Valoracion valoracion);
    }
}