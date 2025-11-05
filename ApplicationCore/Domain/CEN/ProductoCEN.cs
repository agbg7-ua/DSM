using System;
using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    public class ProductoCEN
    {
        private readonly IProductoRepository _productoRepo;
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IUnitOfWork _uow;

        public ProductoCEN(IProductoRepository productoRepo, IUsuarioRepository usuarioRepo, IUnitOfWork uow)
        {
            _productoRepo = productoRepo;
            _usuarioRepo = usuarioRepo;
            _uow = uow;
        }

        public Producto CrearProducto(string nombre, string descripcion, decimal precio, string categoria, ICollection<string> imagenes, int stock, int vendedorId)
        {
            var vendedor = _usuarioRepo.GetById(vendedorId);
            if (vendedor == null)
                throw new Exception($"Usuario con ID {vendedorId} no encontrado");

            var producto = new Producto
            {
                Nombre = nombre,
                Descripcion = descripcion,
                Precio = precio,
                Categoria = categoria,
                Imagenes = imagenes,
                Stock = stock,
                Vendedor = vendedor
            };

            _productoRepo.New(producto);
            _uow.SaveChanges();
            return producto;
        }

        public Producto GetById(long id)
        {
            return _productoRepo.GetById(id);
        }

        public IEnumerable<Producto> BuscarPorNombre(string nombre)
        {
            return _productoRepo.BuscarPorNombre(nombre);
        }
    }
}