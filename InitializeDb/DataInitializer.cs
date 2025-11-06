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

            // Crear direcciones de envío demo
            Console.WriteLine("Creando direcciones de envío demo...");
            var direccionEnvioCEN = scope.ServiceProvider.GetRequiredService<DireccionEnvioCEN>();
            var direccion1 = direccionEnvioCEN.CrearDireccionEnvio(
                "Calle Principal 123",
                "Ciudad Demo",
                "03690",
                "España",
                (int)comprador.Id
            );

            // Crear métodos de pago demo
            Console.WriteLine("Creando métodos de pago demo...");
            var metodoPagoCEN = scope.ServiceProvider.GetRequiredService<MetodoPagoCEN>();
            var metodoPago1 = metodoPagoCEN.CrearMetodoPago(
                "Tarjeta de Crédito",
                "4532-XXXX-XXXX-9876",
                DateTime.Now.AddYears(2), // Expira en 2 años
                (int)comprador.Id
            );

            // Crear pedidos demo
            Console.WriteLine("Creando pedidos demo...");
            var pedidoCEN = scope.ServiceProvider.GetRequiredService<PedidoCEN>();
            var pedidoItemCEN = scope.ServiceProvider.GetRequiredService<PedidoItemCEN>();

            // Pedido 1: Pendiente con varios items
            var pedido1 = pedidoCEN.CrearPedido(
                (int)comprador.Id,
                0, // El total se actualizará al añadir items
                (int)direccion1.Id,
                (int)metodoPago1.Id
            );

            // Añadir items al pedido
            pedidoItemCEN.AgregarItem((int)pedido1.Id, (int)producto1.Id, 1, producto1.Precio); // Una laptop
            pedidoItemCEN.AgregarItem((int)pedido1.Id, (int)producto3.Id, 2, producto3.Precio); // Dos audífonos

            // Pedido 2: En proceso
            var pedido2 = pedidoCEN.CrearPedido(
                (int)comprador.Id,
                0,
                (int)direccion1.Id,
                (int)metodoPago1.Id
            );
            
            pedidoItemCEN.AgregarItem((int)pedido2.Id, (int)producto2.Id, 1, producto2.Precio); // Un smartphone
            pedidoCEN.ActualizarEstado((int)pedido2.Id, ApplicationCore.Domain.Enums.EstadoPedido.EnProceso);

            // Pedido 3: Enviado
            var pedido3 = pedidoCEN.CrearPedido(
                (int)comprador.Id,
                0,
                (int)direccion1.Id,
                (int)metodoPago1.Id
            );
            pedidoItemCEN.AgregarItem((int)pedido3.Id, (int)producto2.Id, 2, producto2.Precio); // Dos smartphones
            pedidoItemCEN.AgregarItem((int)pedido3.Id, (int)producto3.Id, 1, producto3.Precio); // Un audífono
            pedidoCEN.ActualizarEstado((int)pedido3.Id, ApplicationCore.Domain.Enums.EstadoPedido.Enviado);

            // Pedido 4: Entregado
            var pedido4 = pedidoCEN.CrearPedido(
                (int)comprador.Id,
                0,
                (int)direccion1.Id,
                (int)metodoPago1.Id
            );
            pedidoItemCEN.AgregarItem((int)pedido4.Id, (int)producto3.Id, 3, producto3.Precio); // Tres audífonos
            pedidoCEN.ActualizarEstado((int)pedido4.Id, ApplicationCore.Domain.Enums.EstadoPedido.Entregado);

            // Pedido 5: Cancelado
            var pedido5 = pedidoCEN.CrearPedido(
                (int)comprador.Id,
                0,
                (int)direccion1.Id,
                (int)metodoPago1.Id
            );
            pedidoItemCEN.AgregarItem((int)pedido5.Id, (int)producto1.Id, 1, producto1.Precio); // Una laptop
            pedidoCEN.ActualizarEstado((int)pedido5.Id, ApplicationCore.Domain.Enums.EstadoPedido.Cancelado);

            Console.WriteLine("Datos inicializados correctamente.");
        }
    }
}