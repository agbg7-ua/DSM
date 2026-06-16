namespace ApplicationCore.Domain.EN;

public class LineaPrestamo
{
    public virtual long Id { get; set; }
    public virtual long PrestamoId { get; set; }
    public virtual long MaterialId { get; set; }
    public virtual int DiasEstimados { get; set; }
    public virtual Prestamo Prestamo { get; set; } = null!;
    public virtual Material Material { get; set; } = null!;
}
