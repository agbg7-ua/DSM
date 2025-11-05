using System;

namespace ApplicationCore.Domain.EN
{
    public class Valoracion
    {
        public virtual long Id { get; set; }
        public virtual int Puntuacion { get; set; }
        public virtual string Comentario { get; set; }
        public virtual DateTime FechaCreacion { get; set; }

        public virtual Usuario Usuario { get; set; }
        public virtual Producto Producto { get; set; }
    }
}