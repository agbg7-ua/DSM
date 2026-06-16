using ApplicationCore.Domain.EN;

namespace ApplicationCore.Domain.Repositories;

public interface ILineaPrestamoRepository : IRepository<LineaPrestamo, long>
{
    IList<LineaPrestamo> DamePorPrestamo(long prestamoId);
}
