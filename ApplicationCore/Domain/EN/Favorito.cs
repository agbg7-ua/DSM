namespace ApplicationCore.Domain.EN
{
    public class Favorito
    {
        public long IdFavorito { get; set; }

        public virtual Usuario Usuario { get; set; }
        public virtual Producto Producto { get; set; }
    }
}