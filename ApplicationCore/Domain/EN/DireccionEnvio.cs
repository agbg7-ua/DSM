namespace ApplicationCore.Domain.EN
{
    public class DireccionEnvio
    {
        public long IdDireccion { get; set; }
        public string Calle { get; set; }
        public string Ciudad { get; set; }
        public string CodigoPostal { get; set; }
        public string Pais { get; set; }

        public virtual Usuario Usuario { get; set; }
    }
}
