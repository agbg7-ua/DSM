using System.Collections.Generic;

namespace ApplicationCore.Domain.EN
{
    public class Rol
    {
        public long IdRol { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Asociaciones { get; set; }

        public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}
