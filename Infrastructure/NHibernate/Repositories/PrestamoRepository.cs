using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using ApplicationCore.Domain.Repositories;
using NHibernate;

namespace Infrastructure.NHibernate.Repositories;

public class PrestamoRepository : RepositoryBase<Prestamo, long>, IPrestamoRepository
{
    public PrestamoRepository(ISession session) : base(session)
    {
    }

    public IList<Prestamo> DameFilterActivosPorUsuario(long usuarioId) =>
        Session.Query<Prestamo>()
            .Where(p => p.Usuario.Id == usuarioId && p.Estado == EstadoPrestamo.Activo)
            .ToList();

    public IList<Prestamo> DameFilterRetrasados() =>
        Session.Query<Prestamo>()
            .Where(p => p.Estado == EstadoPrestamo.Retrasado)
            .ToList();
}
