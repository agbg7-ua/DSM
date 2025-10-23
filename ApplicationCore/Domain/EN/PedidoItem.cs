using System;

namespace ApplicationCore.Domain.EN
{
    public class PedidoItem
    {
        public long IdItem { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }

        public virtual Pedido Pedido { get; set; }
        public virtual Producto Producto { get; set; }

        public decimal CalcularSubtotal() => Cantidad * PrecioUnitario;
    }
}