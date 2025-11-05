using System;
using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using ApplicationCore.Domain.Enums;

namespace ApplicationCore.Domain.CEN
{
    public class PedidoCEN
    {
        private readonly IPedidoRepository _pedidoRepo;
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IDireccionEnvioRepository _direccionEnvioRepo;
        private readonly IMetodoPagoRepository _metodoPagoRepo;
        private readonly IUnitOfWork _uow;

        public PedidoCEN(
            IPedidoRepository pedidoRepo,
            IUsuarioRepository usuarioRepo,
            IDireccionEnvioRepository direccionEnvioRepo,
            IMetodoPagoRepository metodoPagoRepo,
            IUnitOfWork uow)
        {
            _pedidoRepo = pedidoRepo;
            _usuarioRepo = usuarioRepo;
            _direccionEnvioRepo = direccionEnvioRepo;
            _metodoPagoRepo = metodoPagoRepo;
            _uow = uow;
        }

        public Pedido CrearPedido(int clienteId, decimal total, int direccionEnvioId, int metodoPagoId)
        {
            var cliente = _usuarioRepo.GetById(clienteId);
            if (cliente == null)
                throw new Exception($"Cliente con ID {clienteId} no encontrado");

            var direccionEnvio = _direccionEnvioRepo.GetById(direccionEnvioId);
            if (direccionEnvio == null)
                throw new Exception($"Dirección de envío con ID {direccionEnvioId} no encontrada");

            var metodoPago = _metodoPagoRepo.GetById(metodoPagoId);
            if (metodoPago == null)
                throw new Exception($"Método de pago con ID {metodoPagoId} no encontrado");

            var pedido = new Pedido
            {
                Cliente = cliente,
                PrecioTotal = total,
                DireccionEnvio = direccionEnvio,
                MetodoPago = metodoPago,
                Estado = EstadoPedido.Nuevo,
                Fecha = DateTime.Now
            };

            _pedidoRepo.New(pedido);
            _uow.SaveChanges();
            return pedido;
        }

        public Pedido GetById(int id)
        {
            return _pedidoRepo.GetById(id);
        }

        public IEnumerable<Pedido> GetByCliente(int clienteId)
        {
            return _pedidoRepo.GetByCliente(clienteId);
        }

        public void ActualizarEstado(int pedidoId, EstadoPedido nuevoEstado)
        {
            var pedido = _pedidoRepo.GetById(pedidoId);
            if (pedido == null)
                throw new Exception($"Pedido con ID {pedidoId} no encontrado");

            pedido.Estado = nuevoEstado;
            _uow.SaveChanges();
        }
    }
}