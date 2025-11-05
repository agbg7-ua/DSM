using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using NHibernate;
using System.Collections.Generic;

namespace Infrastructure.Repositories
{
    public class MetodoPagoRepository : NHibernateRepositoryBase<MetodoPago>, IMetodoPagoRepository
    {
        public MetodoPagoRepository(ISession session) : base(session)
        {
        }

        public IEnumerable<MetodoPago> GetByUsuario(long idUsuario)
        {
            return _session.QueryOver<MetodoPago>()
                .Where(mp => mp.Usuario.Id == idUsuario)
                .List();
        }
    }
}