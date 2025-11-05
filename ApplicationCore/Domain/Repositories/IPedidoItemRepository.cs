using ApplicationCore.Domain.EN;
using System.Collections.Generic;

namespace ApplicationCore.Domain.Repositories
{
    public interface IPedidoItemRepository : IRepository<PedidoItem, long>
    {
        IEnumerable<PedidoItem> GetByPedido(long pedidoId);
    }
}