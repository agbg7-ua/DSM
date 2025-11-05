using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using NHibernate;
using System.Collections.Generic;

namespace Infrastructure.Repositories
{
    public class PedidoItemRepository : NHibernateRepositoryBase<PedidoItem>, IPedidoItemRepository
    {
        public PedidoItemRepository(ISession session) : base(session)
        {
        }

        public IEnumerable<PedidoItem> GetByPedido(long pedidoId)
        {
            return _session.QueryOver<PedidoItem>()
                .Where(p => p.Pedido.Id == pedidoId)
                .List();
        }
    }
}