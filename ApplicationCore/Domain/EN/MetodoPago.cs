using System;

namespace ApplicationCore.Domain.EN
{
    public class MetodoPago
    {
        public long IdMetodo { get; set; }
        public string Tipo { get; set; }
        public string Numero { get; set; }
        public DateTime FechaExpiracion { get; set; }

        public virtual Usuario Usuario { get; set; }
    }
}