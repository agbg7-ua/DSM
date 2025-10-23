using NHibernate;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using System.Collections.Generic;

namespace Infrastructure.Repositories
{
    public class ProductoRepository : NHibernateRepositoryBase<Producto>, IProductoRepository
    {
        public ProductoRepository(ISession session) : base(session) { }

        public IEnumerable<Producto> BuscarPorNombre(string nombre)
        {
            return _session.QueryOver<Producto>().Where(p => p.Nombre.IsLike(nombre)).List();
        }
    }
}
