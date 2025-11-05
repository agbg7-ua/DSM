using System;
using Microsoft.Extensions.DependencyInjection;
using ApplicationCore.Domain.CEN;

namespace InitializeDb
{
    public class DataInitializer
    {
        private readonly IServiceProvider _serviceProvider;

        public DataInitializer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void InitializeData()
        {
            using var scope = _serviceProvider.CreateScope();
            var usuarioCEN = scope.ServiceProvider.GetRequiredService<UsuarioCEN>();
            var productoCEN = scope.ServiceProvider.GetRequiredService<ProductoCEN>();

            // Crear roles básicos
            Console.WriteLine("Creando roles básicos...");
            var adminRol = usuarioCEN.CrearRol("Administrador", "Administrador del sistema", "todas");
            var vendedorRol = usuarioCEN.CrearRol("Vendedor", "Usuario que puede vender productos", "productos");
            var compradorRol = usuarioCEN.CrearRol("Comprador", "Usuario que puede comprar productos", "compras");

            // Crear usuarios demo
            Console.WriteLine("Creando usuarios demo...");
            var admin = usuarioCEN.CrearUsuario("admin", "admin@example.com", "admin123", "Calle Admin 123", (int)adminRol.Id);
            var vendedor = usuarioCEN.CrearUsuario("vendedor", "vendedor@example.com", "vendedor123", "Calle Vendedor 123", (int)vendedorRol.Id);
            var comprador = usuarioCEN.CrearUsuario("comprador", "comprador@example.com", "comprador123", "Calle Comprador 123", (int)compradorRol.Id);

            // Crear algunos productos demo
            Console.WriteLine("Creando productos demo...");
            var producto1 = productoCEN.CrearProducto("Laptop Gaming", "Laptop gaming de alta gama", 1299.99m, "Electrónica", new[] { "laptop.jpg" }, 10, (int)vendedor.Id);
            var producto2 = productoCEN.CrearProducto("Smartphone", "Smartphone último modelo", 799.99m, "Electrónica", new[] { "phone.jpg" }, 20, (int)vendedor.Id);
            var producto3 = productoCEN.CrearProducto("Audífonos Bluetooth", "Audífonos inalámbricos", 129.99m, "Accesorios", new[] { "headphones.jpg" }, 50, (int)vendedor.Id);

            Console.WriteLine("Datos inicializados correctamente.");
        }
    }
}