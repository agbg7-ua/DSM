using ApplicationCore.Domain.EN;
using System.Collections.Generic;

namespace ApplicationCore.Domain.Repositories
{
    public interface IProductoRepository : IRepository<Producto, long>
    {
        IEnumerable<Producto> BuscarPorNombre(string nombre);
    }
}