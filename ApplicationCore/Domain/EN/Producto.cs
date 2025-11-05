using System.Collections.Generic;

namespace ApplicationCore.Domain.EN
{
    public class Producto
    {
        public virtual long Id { get; set; }
        public virtual string Nombre { get; set; }
        public virtual string Descripcion { get; set; }
        public virtual decimal Precio { get; set; }
        public virtual string Categoria { get; set; }
        public virtual ICollection<string> Imagenes { get; set; } = new List<string>();
        public virtual int Stock { get; set; }

        public virtual Usuario Vendedor { get; set; }
        public virtual ICollection<Valoracion> Valoraciones { get; set; } = new List<Valoracion>();
        public virtual ICollection<PedidoItem> PedidoItems { get; set; } = new List<PedidoItem>();
    }
}