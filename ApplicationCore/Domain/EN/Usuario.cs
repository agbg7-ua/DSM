using System.Collections.Generic;

namespace ApplicationCore.Domain.EN
{
    public class Usuario
    {
        public virtual long Id { get; set; }
        public virtual string Nombre { get; set; }
        public virtual string Correo { get; set; }
        public virtual string Contrasena { get; set; }
        public virtual string Direccion { get; set; }

        // Navigation properties
        public virtual Rol Rol { get; set; }
        public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
        public virtual ICollection<Valoracion> Valoraciones { get; set; } = new List<Valoracion>();
        public virtual ICollection<Favorito> Favoritos { get; set; } = new List<Favorito>();
        public virtual ICollection<Producto> ProductosPublicados { get; set; } = new List<Producto>();
        public virtual ICollection<DireccionEnvio> DireccionesEnvio { get; set; } = new List<DireccionEnvio>();
        public virtual ICollection<MetodoPago> MetodosPago { get; set; } = new List<MetodoPago>();
    }
}
