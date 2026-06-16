using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using NHibernate;

namespace Infrastructure.NHibernate.Repositories;

public class UsuarioRepository : RepositoryBase<Usuario, long>, IUsuarioRepository
{
    public UsuarioRepository(ISession session) : base(session)
    {
    }
}
