using System;

namespace ApplicationCore.Domain.EN
{
    public class MetodoPago
    {
        public virtual long Id { get; set; }
        public virtual string Tipo { get; set; }
        public virtual string Numero { get; set; }
        public virtual DateTime FechaExpiracion { get; set; }

        public virtual Usuario Usuario { get; set; }
    }
}