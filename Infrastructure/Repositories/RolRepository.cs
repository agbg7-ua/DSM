using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using NHibernate;

namespace Infrastructure.Repositories
{
    public class RolRepository : NHibernateRepositoryBase<Rol>, IRolRepository
    {
        public RolRepository(ISession session) : base(session)
        {
        }
    }
}