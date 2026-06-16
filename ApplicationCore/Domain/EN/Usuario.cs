using ApplicationCore.Domain.Enums;

namespace ApplicationCore.Domain.EN;

public class Usuario
{
    public virtual long Id { get; set; }
    public virtual string Nombre { get; set; } = string.Empty;
    public virtual string Email { get; set; } = string.Empty;
    public virtual string Contrasenia { get; set; } = string.Empty;
    public virtual RolUsuario Rol { get; set; }
    public virtual ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
}
