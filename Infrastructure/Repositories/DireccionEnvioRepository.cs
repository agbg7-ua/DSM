using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using NHibernate;
using System.Linq;

namespace Infrastructure.Repositories
{
    public class DireccionEnvioRepository : NHibernateRepositoryBase<DireccionEnvio>, IDireccionEnvioRepository
    {
        public DireccionEnvioRepository(ISession session) : base(session)
        {
        }

        public IEnumerable<DireccionEnvio> GetByUsuario(long idUsuario)
        {
            return _session.QueryOver<DireccionEnvio>()
                .Where(d => d.Usuario.Id == idUsuario)
                .List();
        }
    }
}