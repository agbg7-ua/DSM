using System;
using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    public class RolCEN
    {
        private readonly IRolRepository _rolRepo;
        private readonly IUnitOfWork _uow;

        public RolCEN(IRolRepository rolRepo, IUnitOfWork uow)
        {
            _rolRepo = rolRepo;
            _uow = uow;
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

        public void ModificarRol(long id, string? nombre = null, string? descripcion = null, string? asociaciones = null)
        {
            var rol = _rolRepo.GetById(id);
            if (rol == null)
                throw new Exception($"Rol con ID {id} no encontrado.");

            if (nombre != null) rol.Nombre = nombre;
            if (descripcion != null) rol.Descripcion = descripcion;
            if (asociaciones != null) rol.Asociaciones = asociaciones;

            _rolRepo.Modify(rol);
            _uow.SaveChanges();
        }

        public void EliminarRol(long id)
        {
            var rol = _rolRepo.GetById(id);
            if (rol == null)
                throw new Exception($"Rol con ID {id} no encontrado.");

            _rolRepo.Destroy(id);
            _uow.SaveChanges();
        }

        public Rol ObtenerRolPorId(long id)
        {
            var rol = _rolRepo.GetById(id);
            if (rol == null)
                throw new Exception($"Rol con ID {id} no encontrado.");

            return rol;
        }

        public IList<Rol> ObtenerTodosLosRoles()
        {
            return _rolRepo.GetAll();
        }
    }
}
