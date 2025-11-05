using System;
using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    public class DireccionEnvioCEN
    {
        private readonly IDireccionEnvioRepository _direccionEnvioRepo;
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IUnitOfWork _uow;

        public DireccionEnvioCEN(IDireccionEnvioRepository direccionEnvioRepo, IUsuarioRepository usuarioRepo, IUnitOfWork uow)
        {
            _direccionEnvioRepo = direccionEnvioRepo;
            _usuarioRepo = usuarioRepo;
            _uow = uow;
        }

        public DireccionEnvio CrearDireccionEnvio(string calle, string ciudad, string codigoPostal, string pais, int usuarioId)
        {
            var usuario = _usuarioRepo.GetById(usuarioId);
            if (usuario == null)
                throw new Exception($"Usuario con ID {usuarioId} no encontrado");

            var direccionEnvio = new DireccionEnvio
            {
                Calle = calle,
                Ciudad = ciudad,
                CodigoPostal = codigoPostal,
                Pais = pais,
                Usuario = usuario
            };

            _direccionEnvioRepo.New(direccionEnvio);
            _uow.SaveChanges();
            return direccionEnvio;
        }

        public DireccionEnvio GetById(int id)
        {
            return _direccionEnvioRepo.GetById(id);
        }

        public IEnumerable<DireccionEnvio> GetByUsuario(int usuarioId)
        {
            return _direccionEnvioRepo.GetByUsuario(usuarioId);
        }

        public void EliminarDireccionEnvio(int id)
        {
            var direccionEnvio = _direccionEnvioRepo.GetById(id);
            if (direccionEnvio == null)
                throw new Exception($"Dirección de envío con ID {id} no encontrada");

            _direccionEnvioRepo.Destroy(direccionEnvio);
            _uow.SaveChanges();
        }
    }
}