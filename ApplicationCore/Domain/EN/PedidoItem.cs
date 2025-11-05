using System;

namespace ApplicationCore.Domain.EN
{
    public class PedidoItem
    {
        public virtual long Id { get; set; }
        public virtual int Cantidad { get; set; }
        public virtual decimal PrecioUnitario { get; set; }

        public virtual Pedido Pedido { get; set; }
        public virtual Producto Producto { get; set; }

        public virtual decimal CalcularSubtotal() => Cantidad * PrecioUnitario;
    }
}