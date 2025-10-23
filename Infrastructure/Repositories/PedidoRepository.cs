using NHibernate;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using System.Collections.Generic;

namespace Infrastructure.Repositories
{
    public class PedidoRepository : NHibernateRepositoryBase<Pedido>, IPedidoRepository
    {
        public PedidoRepository(ISession session) : base(session) { }

        public IEnumerable<Pedido> GetByCliente(long clienteId)
        {
            return _session.QueryOver<Pedido>().Where(p => p.Cliente.IdUsuario == clienteId).List();
        }
    }
}
