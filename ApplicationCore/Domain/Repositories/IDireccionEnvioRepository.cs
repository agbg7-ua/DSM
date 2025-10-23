using ApplicationCore.Domain.EN;
using System.Collections.Generic;

namespace ApplicationCore.Domain.Repositories
{
    public interface IDireccionEnvioRepository : IRepository<DireccionEnvio, long>
    {
        IEnumerable<DireccionEnvio> GetByUsuario(long usuarioId);
    }
}