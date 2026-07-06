using ApplicationCore.Domain.Enums;

namespace ApplicationCore.Domain.EN;

public class Usuario
{
    public virtual long Id { get; set; }
    public virtual string Nombre { get; set; } = string.Empty;
    public virtual string Email { get; set; } = string.Empty;
    public virtual string Contrasenia { get; set; } = string.Empty;
    public virtual RolUsuario Rol { get; set; }

    // Vinculación con un proveedor externo OAuth2 / OpenID Connect (Google, Microsoft,
    // Keycloak, Auth0, etc.). Null/vacío en cuentas creadas con email + contraseña local.
    public virtual string? ProveedorExterno { get; set; }
    public virtual string? IdExterno { get; set; }

    public virtual ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
}
