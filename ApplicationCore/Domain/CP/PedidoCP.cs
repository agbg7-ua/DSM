using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using System.Linq;
using System;

namespace ApplicationCore.Domain.CP
{
    // Ejemplo simple de CP que crea un pedido a partir de items y usuario
    public class PedidoCP
    {
        private readonly IPedidoRepository _pedidoRepo;
        private readonly IUnitOfWork _uow;

        public PedidoCP(IPedidoRepository pedidoRepo, IUnitOfWork uow)
        {
            _pedidoRepo = pedidoRepo;
            _uow = uow;
        }

        public Pedido CrearPedido(Usuario cliente, DireccionEnvio direccion, MetodoPago metodo, params PedidoItem[] items)
        {
            var pedido = new Pedido
            {
                Cliente = cliente,
                DireccionEnvio = direccion,
                MetodoPago = metodo,
                Fecha = DateTime.UtcNow,
                Estado = Enums.EstadoPedido.PENDIENTE
            };

            decimal total = 0;
            foreach (var it in items)
            {
                pedido.Items.Add(it);
                total += it.CalcularSubtotal();
            }
            pedido.PrecioTotal = total;

            _pedidoRepo.New(pedido);
            _uow.SaveChanges();

            return pedido;
        }
    }
}