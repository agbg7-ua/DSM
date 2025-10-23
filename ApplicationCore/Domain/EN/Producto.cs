using System.Collections.Generic;

namespace ApplicationCore.Domain.EN
{
    public class Producto
    {
        public long IdProducto { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public string Categoria { get; set; }
        public ICollection<string> Imagenes { get; set; } = new List<string>();
        public int Stock { get; set; }

        public virtual Usuario Vendedor { get; set; }
        public virtual ICollection<Valoracion> Valoraciones { get; set; } = new List<Valoracion>();
        public virtual ICollection<PedidoItem> PedidoItems { get; set; } = new List<PedidoItem>();
    }
}