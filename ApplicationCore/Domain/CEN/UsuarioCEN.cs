using System;
using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    public class UsuarioCEN
    {
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IRolRepository _rolRepo;
        private readonly IUnitOfWork _uow;

        public UsuarioCEN(IUsuarioRepository usuarioRepo, IRolRepository rolRepo, IUnitOfWork uow)
        {
            _usuarioRepo = usuarioRepo;
            _rolRepo = rolRepo;
            _uow = uow;
        }

        public Usuario CrearUsuario(string nombre, string correo, string contrasena, string direccion, int rolId)
        {
            var rol = _rolRepo.GetById(rolId);
            if (rol == null)
                throw new Exception($"Rol con ID {rolId} no encontrado");

            var usuario = new Usuario
            {
                Nombre = nombre,
                Correo = correo,
                Contrasena = contrasena, // En una aplicación real, esto debería estar hasheado
                Direccion = direccion,
                Rol = rol
            };

            _usuarioRepo.New(usuario);
            _uow.SaveChanges();
            return usuario;
        }

        public Rol CrearRol(string nombre, string descripcion, string asociaciones)
        {
            var rol = new Rol
            {
                Nombre = nombre,
                Descripcion = descripcion,
                Asociaciones = asociaciones
            };

            _rolRepo.New(rol);
            _uow.SaveChanges();
            return rol;
        }
    }
}