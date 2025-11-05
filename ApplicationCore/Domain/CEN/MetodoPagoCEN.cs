using System;
using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    public class MetodoPagoCEN
    {
        private readonly IMetodoPagoRepository _metodoPagoRepo;
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IUnitOfWork _uow;

        public MetodoPagoCEN(IMetodoPagoRepository metodoPagoRepo, IUsuarioRepository usuarioRepo, IUnitOfWork uow)
        {
            _metodoPagoRepo = metodoPagoRepo;
            _usuarioRepo = usuarioRepo;
            _uow = uow;
        }

        public MetodoPago CrearMetodoPago(string tipo, string numero, DateTime fechaExpiracion, int usuarioId)
        {
            var usuario = _usuarioRepo.GetById(usuarioId);
            if (usuario == null)
                throw new Exception($"Usuario con ID {usuarioId} no encontrado");

            var metodoPago = new MetodoPago
            {
                Tipo = tipo,
                Numero = numero,
                FechaExpiracion = fechaExpiracion,
                Usuario = usuario
            };

            _metodoPagoRepo.New(metodoPago);
            _uow.SaveChanges();
            return metodoPago;
        }

        public MetodoPago GetById(int id)
        {
            return _metodoPagoRepo.GetById(id);
        }

        public IEnumerable<MetodoPago> GetByUsuario(int usuarioId)
        {
            return _metodoPagoRepo.GetByUsuario(usuarioId);
        }

        public void EliminarMetodoPago(int id)
        {
            var metodoPago = _metodoPagoRepo.GetById(id);
            if (metodoPago == null)
                throw new Exception($"Método de pago con ID {id} no encontrado");

            _metodoPagoRepo.Destroy(metodoPago);
            _uow.SaveChanges();
        }
    }
}