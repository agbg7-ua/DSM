using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    public class ProductoCEN
    {
        private readonly IProductoRepository _productoRepo;
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IValoracionRepository _valoracionRepo;
        private readonly IUnitOfWork _uow;

        public ProductoCEN(
            IProductoRepository productoRepo,
            IUsuarioRepository usuarioRepo,
            IValoracionRepository valoracionRepo,
            IUnitOfWork uow)
        {
            _productoRepo = productoRepo;
            _usuarioRepo = usuarioRepo;
            _valoracionRepo = valoracionRepo;
            _uow = uow;
        }

        public Producto CrearProducto(string nombre, string descripcion, decimal precio, string categoria, ICollection<string> imagenes, int stock, long vendedorId)
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

        public void ModificarProducto(long id, string? nombre = null, string? descripcion = null,
                                      decimal? precio = null, string? categoria = null,
                                      ICollection<string>? imagenes = null, int? stock = null)
        {
            var producto = _productoRepo.GetById(id);
            if (producto == null)
                throw new Exception($"Producto con ID {id} no encontrado");

            if (nombre != null) producto.Nombre = nombre;
            if (descripcion != null) producto.Descripcion = descripcion;
            if (precio.HasValue) producto.Precio = precio.Value;
            if (categoria != null) producto.Categoria = categoria;
            if (imagenes != null) producto.Imagenes = imagenes;
            if (stock.HasValue) producto.Stock = stock.Value;

            _productoRepo.Modify(producto);
            _uow.SaveChanges();
        }

        public void EliminarProducto(long id)
        {
            var producto = _productoRepo.GetById(id);
            if (producto == null)
                throw new Exception($"Producto con ID {id} no encontrado");

            _productoRepo.Destroy(id);
            _uow.SaveChanges();
        }

        public Producto GetById(long id)
        {
            return _productoRepo.GetById(id);
        }

        public IEnumerable<Producto> GetAll()
        {
            return _productoRepo.GetAll();
        }

        public IEnumerable<Producto> BuscarPorNombre(string nombre)
        {
            return _productoRepo.GetAll()
                .Where(p => p.Nombre.Contains(nombre, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public IEnumerable<Producto> FiltrarPorCategoria(string categoria)
        {
            return _productoRepo.GetAll()
                .Where(p => p.Categoria.Equals(categoria, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public IEnumerable<Producto> FiltrarPorRangoPrecio(decimal precioMin, decimal precioMax)
        {
            return _productoRepo.GetAll()
                .Where(p => p.Precio >= precioMin && p.Precio <= precioMax)
                .ToList();
        }

        public decimal CalcularValoracionMedia(long productoId)
        {
            var valoraciones = _valoracionRepo.GetAll()
                .Where(v => v.Producto.Id == productoId)
                .ToList();

            if (valoraciones.Count == 0)
                return 0;

            return (decimal)valoraciones.Average(v => v.Puntuacion);
        }

        public IEnumerable<Producto> GetTopValorados(int topN = 5)
        {
            var productos = _productoRepo.GetAll();
            var valoraciones = _valoracionRepo.GetAll();

            var resultado = productos
                .Select(p => new
                {
                    Producto = p,
                    Media = valoraciones
                        .Where(v => v.Producto.Id == p.Id)
                        .Select(v => v.Puntuacion)
                        .DefaultIfEmpty(0)
                        .Average()
                })
                .OrderByDescending(x => x.Media)
                .Take(topN)
                .Select(x => x.Producto)
                .ToList();

            return resultado;
        }

        public IEnumerable<Producto> GetProductosConPocoStock(int umbral = 5)
        {
            return _productoRepo.GetAll()
                .Where(p => p.Stock <= umbral)
                .ToList();
        }
    }
}
