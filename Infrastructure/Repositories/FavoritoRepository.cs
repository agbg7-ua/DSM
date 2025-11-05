using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using NHibernate;
using System.Collections.Generic;

namespace Infrastructure.Repositories
{
    public class FavoritoRepository : NHibernateRepositoryBase<Favorito>, IFavoritoRepository
    {
        public FavoritoRepository(ISession session) : base(session)
        {
        }

        public IEnumerable<Favorito> GetByUsuario(long idUsuario)
        {
            return _session.QueryOver<Favorito>()
                .Where(f => f.Usuario.Id == idUsuario)
                .List();
        }

        public bool ExisteFavorito(long usuarioId, long productoId)
        {
            return _session.QueryOver<Favorito>()
                .Where(f => f.Usuario.Id == usuarioId && f.Producto.Id == productoId)
                .RowCount() > 0;
        }

        public Favorito GetFavorito(long usuarioId, long productoId)
        {
            return _session.QueryOver<Favorito>()
                .Where(f => f.Usuario.Id == usuarioId && f.Producto.Id == productoId)
                .SingleOrDefault();
        }

        public void Delete(Favorito favorito)
        {
            _session.Delete(favorito);
        }
    }
}