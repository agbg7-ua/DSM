namespace ApplicationCore.Domain.EN
{
    public class Favorito
    {
        public virtual long Id { get; set; }

        public virtual Usuario Usuario { get; set; }
        public virtual Producto Producto { get; set; }
    }
}