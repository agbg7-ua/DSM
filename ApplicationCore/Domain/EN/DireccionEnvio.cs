namespace ApplicationCore.Domain.EN
{
    public class DireccionEnvio
    {
        public virtual long Id { get; set; }
        public virtual string Calle { get; set; }
        public virtual string Ciudad { get; set; }
        public virtual string CodigoPostal { get; set; }
        public virtual string Pais { get; set; }

        public virtual Usuario Usuario { get; set; }
    }
}
