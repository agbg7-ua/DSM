using ApplicationCore.Domain.EN;
using System.Collections.Generic;

namespace ApplicationCore.Domain.Repositories
{
    public interface IFavoritoRepository : IRepository<Favorito, long>
    {
        IEnumerable<Favorito> GetByUsuario(long usuarioId);
        bool ExisteFavorito(long usuarioId, long productoId);
        Favorito GetFavorito(long usuarioId, long productoId);
        void Delete(Favorito favorito);
    }
}