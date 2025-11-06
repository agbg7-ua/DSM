using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using ApplicationCore.Domain.Utils; // para PasswordHelper

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

        // ========================
        // CRUD BASICOS
        // ========================

        public Usuario New(string nombre, string correo, string contrasena, long rolId, string? direccion = null)
        {
            var rol = _rolRepo.GetById(rolId);
            if (rol == null)
                throw new Exception($"Rol con ID {rolId} no encontrado");

            var hashedPass = PasswordHelper.HashPassword(contrasena);

            var usuario = new Usuario
            {
                Nombre = nombre,
                Correo = correo,
                Contrasena = hashedPass,
                Direccion = direccion,
                Rol = rol
            };

            _usuarioRepo.New(usuario);
            _uow.SaveChanges();
            return usuario;
        }

        public void Modify(long idUsuario, string? nombre = null, string? correo = null, string? direccion = null)
        {
            var usuario = _usuarioRepo.GetById(idUsuario) ?? throw new Exception("Usuario no encontrado");

            if (!string.IsNullOrWhiteSpace(nombre))
                usuario.Nombre = nombre;
            if (!string.IsNullOrWhiteSpace(correo))
                usuario.Correo = correo;
            if (direccion != null)
                usuario.Direccion = direccion;

            _usuarioRepo.Modify(usuario);
            _uow.SaveChanges();
        }

        public void Destroy(long idUsuario)
        {
            var usuario = _usuarioRepo.GetById(idUsuario);
            if (usuario != null)
            {
                _usuarioRepo.Destroy(usuario);
                _uow.SaveChanges();
            }
        }

        public Usuario ReadOID(long idUsuario)
        {
            return _usuarioRepo.GetById(idUsuario);
        }

        public IList<Usuario> ReadAll()
        {
            return _usuarioRepo.GetAll();
        }

        // ========================
        // LOGIN
        // ========================

        public Usuario Login(string correo, string contrasena)
        {
            var usuario = _usuarioRepo.GetByEmail(correo);
            if (usuario == null)
                throw new Exception("Usuario no encontrado");

            var valid = PasswordHelper.VerifyPassword(contrasena, usuario.Contrasena);
            if (!valid)
                throw new Exception("Contraseña incorrecta");

            return usuario;
        }

        // ========================
        // FILTROS (readFilter)
        // ========================

        // 1️⃣ Usuarios por Rol
        public IList<Usuario> ReadFilterByRol(long rolId)
        {
            return _usuarioRepo.GetByRol(rolId);
        }

        // 2️⃣ Usuarios con dirección no vacía
        public IList<Usuario> ReadFilterWithDireccion()
        {
            return _usuarioRepo.GetAll().Where(u => !string.IsNullOrWhiteSpace(u.Direccion)).ToList();
        }

        // 3️⃣ Buscar usuarios por nombre (case insensitive)
        public IList<Usuario> ReadFilterByNombre(string nombreParcial)
        {
            return _usuarioRepo.GetAll()
                .Where(u => u.Nombre.Contains(nombreParcial, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // ========================
        // CUSTOM OPERATIONS
        // ========================

        // Custom 1: Cambiar contraseña (segura)
        public void CambiarContrasena(long idUsuario, string contrasenaActual, string nuevaContrasena)
        {
            var usuario = _usuarioRepo.GetById(idUsuario) ?? throw new Exception("Usuario no encontrado");

            if (!PasswordHelper.VerifyPassword(contrasenaActual, usuario.Contrasena))
                throw new Exception("Contraseña actual incorrecta");

            usuario.Contrasena = PasswordHelper.HashPassword(nuevaContrasena);
            _usuarioRepo.Modify(usuario);
            _uow.SaveChanges();
        }

        // Custom 2: Desactivar cuenta (soft delete)
        public void DesactivarCuenta(long idUsuario)
        {
            var usuario = _usuarioRepo.GetById(idUsuario) ?? throw new Exception("Usuario no encontrado");
            usuario.Activo = false;
            _usuarioRepo.Modify(usuario);
            _uow.SaveChanges();
        }

        // Custom 3: Contar usuarios por rol
        public int ContarUsuariosPorRol(long rolId)
        {
            return _usuarioRepo.GetByRol(rolId).Count;
        }
    }
}
