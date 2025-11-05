using System.Collections.Generic;

namespace ApplicationCore.Domain.EN
{
    public class Rol
    {
        public virtual long Id { get; set; }
        public virtual string Nombre { get; set; }
        public virtual string Descripcion { get; set; }
        public virtual string Asociaciones { get; set; }

        public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}
