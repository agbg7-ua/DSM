using ApplicationCore.Domain.Enums;

namespace ApplicationCore.Domain.EN;

public class Prestamo
{
    public virtual long Id { get; set; }
    public virtual long UsuarioId { get; set; }
    public virtual DateTime FechaCreacion { get; set; }
    public virtual EstadoPrestamo Estado { get; set; }
    public virtual int TotalDias { get; set; }
    public virtual Usuario Usuario { get; set; } = null!;
    public virtual ICollection<LineaPrestamo> LineasPrestamo { get; set; } = new List<LineaPrestamo>();
}
