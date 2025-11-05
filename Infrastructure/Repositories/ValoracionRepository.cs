using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using NHibernate;
using System.Collections.Generic;

namespace Infrastructure.Repositories
{
    public class ValoracionRepository : NHibernateRepositoryBase<Valoracion>, IValoracionRepository
    {
        public ValoracionRepository(ISession session) : base(session)
        {
        }

        public IEnumerable<Valoracion> GetByProducto(long idProducto)
        {
            return _session.QueryOver<Valoracion>()
                .Where(v => v.Producto.Id == idProducto)
                .List();
        }

        public bool ExisteValoracion(long usuarioId, long productoId)
        {
            return _session.QueryOver<Valoracion>()
                .Where(v => v.Usuario.Id == usuarioId && v.Producto.Id == productoId)
                .RowCount() > 0;
        }

        public IEnumerable<Valoracion> GetByUsuario(long usuarioId)
        {
            return _session.QueryOver<Valoracion>()
                .Where(v => v.Usuario.Id == usuarioId)
                .List();
        }

        public void Delete(Valoracion valoracion)
        {
            _session.Delete(valoracion);
        }
    }
}