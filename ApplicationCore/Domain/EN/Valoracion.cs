namespace ApplicationCore.Domain.EN
{
    public class Valoracion
    {
        public long IdValoracion { get; set; }
        public int Puntuacion { get; set; }
        public string Comentario { get; set; }

        public virtual Usuario Usuario { get; set; }
        public virtual Producto Producto { get; set; }
    }
}