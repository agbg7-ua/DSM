using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;

namespace ApplicationCore.Domain.Repositories;

public interface IMaterialRepository : IRepository<Material, long>
{
    IList<Material> DameFilterDisponibles();
    IList<Material> DameFilterPorEstado(EstadoMaterial estado);
}
