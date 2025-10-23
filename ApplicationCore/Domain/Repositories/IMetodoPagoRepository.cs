using ApplicationCore.Domain.EN;
using System.Collections.Generic;

namespace ApplicationCore.Domain.Repositories
{
    public interface IMetodoPagoRepository : IRepository<MetodoPago, long>
    {
        IEnumerable<MetodoPago> GetByUsuario(long usuarioId);
    }
}