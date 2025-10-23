using NHibernate;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using System.Linq;

namespace Infrastructure.Repositories
{
    public class UsuarioRepository : NHibernateRepositoryBase<Usuario>, IUsuarioRepository
    {
        public UsuarioRepository(ISession session) : base(session) { }

        public Usuario GetByCorreo(string correo)
        {
            return _session.QueryOver<Usuario>().Where(u => u.Correo == correo).SingleOrDefault();
        }
    }
}
