using ApplicationCore.Domain.EN;

namespace ApplicationCore.Domain.Repositories;

public interface IPrestamoRepository : IRepository<Prestamo, long>
{
    IList<Prestamo> DameFilterActivosPorUsuario(long usuarioId);
    IList<Prestamo> DameFilterRetrasados();
}
