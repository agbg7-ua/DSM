using ApplicationCore.Domain.Enums;

namespace ApplicationCore.Domain.EN;

public class Material
{
    public virtual long Id { get; set; }
    public virtual string Nombre { get; set; } = string.Empty;
    public virtual string Descripcion { get; set; } = string.Empty;
    public virtual EstadoMaterial Estado { get; set; }
    public virtual bool EstaDisponible { get; set; }
    public virtual string Imagen { get; set; } = string.Empty;
    public virtual long? UsuarioId { get; set; }
    public virtual Usuario? UsuarioAsignado { get; set; }
    public virtual ICollection<LineaPrestamo> LineasPrestamo { get; set; } = new List<LineaPrestamo>();
}
