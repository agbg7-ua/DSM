using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using NHibernate;

namespace Infrastructure.NHibernate.Repositories;

public class LineaPrestamoRepository : RepositoryBase<LineaPrestamo, long>, ILineaPrestamoRepository
{
    public LineaPrestamoRepository(ISession session) : base(session)
    {
    }

    public IList<LineaPrestamo> DamePorPrestamo(long prestamoId) =>
        Session.Query<LineaPrestamo>()
            .Where(l => l.Prestamo.Id == prestamoId)
            .ToList();
}
