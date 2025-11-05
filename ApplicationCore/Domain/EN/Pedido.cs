using ApplicationCore.Domain.Enums;
using System.Collections.Generic;
using System;

namespace ApplicationCore.Domain.EN
{
    public class Pedido
    {
        public virtual long Id { get; set; }
        public virtual DateTime Fecha { get; set; }
        public virtual EstadoPedido Estado { get; set; }
        public virtual decimal PrecioTotal { get; set; }

        public virtual Usuario Cliente { get; set; }
        public virtual ICollection<PedidoItem> Items { get; set; } = new List<PedidoItem>();
        public virtual DireccionEnvio DireccionEnvio { get; set; }
        public virtual MetodoPago MetodoPago { get; set; }
    }
}