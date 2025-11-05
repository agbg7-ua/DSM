using System;
using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    public class PedidoItemCEN
    {
        private readonly IPedidoItemRepository _pedidoItemRepo;
        private readonly IPedidoRepository _pedidoRepo;
        private readonly IProductoRepository _productoRepo;
        private readonly IUnitOfWork _uow;

        public PedidoItemCEN(
            IPedidoItemRepository pedidoItemRepo,
            IPedidoRepository pedidoRepo,
            IProductoRepository productoRepo,
            IUnitOfWork uow)
        {
            _pedidoItemRepo = pedidoItemRepo;
            _pedidoRepo = pedidoRepo;
            _productoRepo = productoRepo;
            _uow = uow;
        }

        public PedidoItem AgregarItem(int pedidoId, int productoId, int cantidad, decimal precioUnitario)
        {
            var pedido = _pedidoRepo.GetById(pedidoId);
            if (pedido == null)
                throw new Exception($"Pedido con ID {pedidoId} no encontrado");

            var producto = _productoRepo.GetById(productoId);
            if (producto == null)
                throw new Exception($"Producto con ID {productoId} no encontrado");

            if (cantidad <= 0)
                throw new Exception("La cantidad debe ser mayor que cero");

            var item = new PedidoItem
            {
                Pedido = pedido,
                Producto = producto,
                Cantidad = cantidad,
                PrecioUnitario = precioUnitario
            };

            _pedidoItemRepo.New(item);
            
            // Actualizar el total del pedido
            pedido.PrecioTotal += item.CalcularSubtotal();
            
            _uow.SaveChanges();
            return item;
        }

        public void EliminarItem(int itemId)
        {
            var item = _pedidoItemRepo.GetById(itemId);
            if (item == null)
                throw new Exception($"Item de pedido con ID {itemId} no encontrado");

            // Actualizar el total del pedido
            item.Pedido.PrecioTotal -= item.CalcularSubtotal();

            _pedidoItemRepo.Destroy(item);
            _uow.SaveChanges();
        }

        public void ActualizarCantidad(int itemId, int nuevaCantidad)
        {
            if (nuevaCantidad <= 0)
                throw new Exception("La cantidad debe ser mayor que cero");

            var item = _pedidoItemRepo.GetById(itemId);
            if (item == null)
                throw new Exception($"Item de pedido con ID {itemId} no encontrado");

            // Actualizar el total del pedido
            item.Pedido.PrecioTotal -= item.CalcularSubtotal();
            
            item.Cantidad = nuevaCantidad;
            
            item.Pedido.PrecioTotal += item.CalcularSubtotal();

            _uow.SaveChanges();
        }

        public IEnumerable<PedidoItem> GetByPedido(int pedidoId)
        {
            return _pedidoItemRepo.GetByPedido(pedidoId);
        }
    }
}