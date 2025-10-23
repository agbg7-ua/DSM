using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    public class ProductoCEN
    {
        private readonly IProductoRepository _productoRepo;
        private readonly IUnitOfWork _uow;

        public ProductoCEN(IProductoRepository productoRepo, IUnitOfWork uow)
        {
            _productoRepo = productoRepo;
            _uow = uow;
        }

        public void Crear(string nombre, decimal precio, int stock, string? descripcion = null, string? categoria = null)
        {
            var p = new Producto
            {
                Nombre = nombre,
                Precio = precio,
                Stock = stock,
                Descripcion = descripcion,
                Categoria = categoria,
                Imagenes = new List<string>()
            };

            _productoRepo.New(p);
            _uow.SaveChanges();
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