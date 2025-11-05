using System;
using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    public class ValoracionCEN
    {
        private readonly IValoracionRepository _valoracionRepo;
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IProductoRepository _productoRepo;
        private readonly IUnitOfWork _uow;

        public ValoracionCEN(
            IValoracionRepository valoracionRepo,
            IUsuarioRepository usuarioRepo,
            IProductoRepository productoRepo,
            IUnitOfWork uow)
        {
            _valoracionRepo = valoracionRepo;
            _usuarioRepo = usuarioRepo;
            _productoRepo = productoRepo;
            _uow = uow;
        }

        public Valoracion CrearValoracion(string comentario, int puntuacion, int usuarioId, int productoId)
        {
            if (puntuacion < 1 || puntuacion > 5)
                throw new Exception("La puntuación debe estar entre 1 y 5");

            var usuario = _usuarioRepo.GetById(usuarioId);
            if (usuario == null)
                throw new Exception($"Usuario con ID {usuarioId} no encontrado");

            var producto = _productoRepo.GetById(productoId);
            if (producto == null)
                throw new Exception($"Producto con ID {productoId} no encontrado");

            // Verificar si el usuario ya ha valorado este producto
            if (_valoracionRepo.ExisteValoracion(usuarioId, productoId))
                throw new Exception("Ya has valorado este producto");

            var valoracion = new Valoracion
            {
                Comentario = comentario,
                Puntuacion = puntuacion,
                Usuario = usuario,
                Producto = producto,
                FechaCreacion = DateTime.Now
            };

            _valoracionRepo.New(valoracion);
            _uow.SaveChanges();
            return valoracion;
        }

        public void EliminarValoracion(int id)
        {
            var valoracion = _valoracionRepo.GetById(id);
            if (valoracion == null)
                throw new Exception($"Valoración con ID {id} no encontrada");

            _valoracionRepo.Destroy(valoracion);
            _uow.SaveChanges();
        }

        public IEnumerable<Valoracion> GetByProducto(int productoId)
        {
            return _valoracionRepo.GetByProducto(productoId);
        }

        public IEnumerable<Valoracion> GetByUsuario(int usuarioId)
        {
            return _valoracionRepo.GetByUsuario(usuarioId);
        }

        public double GetPromedioPuntuacionProducto(int productoId)
        {
            var valoraciones = _valoracionRepo.GetByProducto(productoId);
            int total = 0;
            int count = 0;

            foreach (var valoracion in valoraciones)
            {
                total += valoracion.Puntuacion;
                count++;
            }

            return count > 0 ? (double)total / count : 0;
        }
    }
}