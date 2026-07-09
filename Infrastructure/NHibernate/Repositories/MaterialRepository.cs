using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using ApplicationCore.Domain.Repositories;
using NHibernate;

namespace Infrastructure.NHibernate.Repositories;

public class MaterialRepository : RepositoryBase<Material, long>, IMaterialRepository
{
    public MaterialRepository(ISession session) : base(session)
    {
    }

    public IList<Material> DameFilterPorEstado(EstadoMaterial estado) =>
        Session.Query<Material>()
            .Where(m => m.Estado == estado)
            .ToList();

    public Material? DamePorOIDConBloqueo(long id) =>
        Session.Get<Material>(id, LockMode.Upgrade);
}
