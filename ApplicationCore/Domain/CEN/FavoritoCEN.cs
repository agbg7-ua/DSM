using System;
using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    public class FavoritoCEN
    {
        private readonly IFavoritoRepository _favoritoRepo;
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IProductoRepository _productoRepo;
        private readonly IUnitOfWork _uow;

        public FavoritoCEN(IFavoritoRepository favoritoRepo, IUsuarioRepository usuarioRepo, 
            IProductoRepository productoRepo, IUnitOfWork uow)
        {
            _favoritoRepo = favoritoRepo;
            _usuarioRepo = usuarioRepo;
            _productoRepo = productoRepo;
            _uow = uow;
        }

        public Favorito AgregarFavorito(int usuarioId, int productoId)
        {
            var usuario = _usuarioRepo.GetById(usuarioId);
            if (usuario == null)
                throw new Exception($"Usuario con ID {usuarioId} no encontrado");

            var producto = _productoRepo.GetById(productoId);
            if (producto == null)
                throw new Exception($"Producto con ID {productoId} no encontrado");

            // Verificar si ya existe el favorito
            if (_favoritoRepo.ExisteFavorito(usuarioId, productoId))
                throw new Exception("Este producto ya está en favoritos");

            var favorito = new Favorito
            {
                Usuario = usuario,
                Producto = producto
            };

            _favoritoRepo.New(favorito);
            _uow.SaveChanges();
            return favorito;
        }

        public void EliminarFavorito(int usuarioId, int productoId)
        {
            var favorito = _favoritoRepo.GetFavorito(usuarioId, productoId);
            if (favorito == null)
                throw new Exception("Favorito no encontrado");

            _favoritoRepo.Destroy(favorito);
            _uow.SaveChanges();
        }

        public IEnumerable<Favorito> GetFavoritosByUsuario(int usuarioId)
        {
            return _favoritoRepo.GetByUsuario(usuarioId);
        }
    }
}