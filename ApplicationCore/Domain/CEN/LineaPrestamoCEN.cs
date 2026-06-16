using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN;

public class LineaPrestamoCEN
{
    private readonly ILineaPrestamoRepository _repository;
    private readonly IPrestamoRepository _prestamoRepository;
    private readonly IMaterialRepository _materialRepository;

    public LineaPrestamoCEN(
        ILineaPrestamoRepository repository,
        IPrestamoRepository prestamoRepository,
        IMaterialRepository materialRepository)
    {
        _repository = repository;
        _prestamoRepository = prestamoRepository;
        _materialRepository = materialRepository;
    }

    public long Crear(long prestamoId, long materialId, int diasEstimados)
    {
        var prestamo = _prestamoRepository.DamePorOID(prestamoId)
            ?? throw new InvalidOperationException($"Prestamo con id {prestamoId} no encontrado.");
        var material = _materialRepository.DamePorOID(materialId)
            ?? throw new InvalidOperationException($"Material con id {materialId} no encontrado.");

        var linea = new LineaPrestamo
        {
            Prestamo = prestamo,
            Material = material,
            DiasEstimados = diasEstimados
        };
        return _repository.New(linea);
    }

    public void Modificar(long id, long prestamoId, long materialId, int diasEstimados)
    {
        var linea = _repository.DamePorOID(id)
            ?? throw new InvalidOperationException($"LineaPrestamo con id {id} no encontrada.");

        var prestamo = _prestamoRepository.DamePorOID(prestamoId)
            ?? throw new InvalidOperationException($"Prestamo con id {prestamoId} no encontrado.");
        var material = _materialRepository.DamePorOID(materialId)
            ?? throw new InvalidOperationException($"Material con id {materialId} no encontrado.");

        linea.Prestamo = prestamo;
        linea.Material = material;
        linea.DiasEstimados = diasEstimados;
        _repository.Modify(linea);
    }

    public void Eliminar(long id)
    {
        var linea = _repository.DamePorOID(id)
            ?? throw new InvalidOperationException($"LineaPrestamo con id {id} no encontrada.");
        _repository.Destroy(linea);
    }

    public LineaPrestamo? ObtenerPorId(long id) => _repository.DamePorOID(id);

    public IList<LineaPrestamo> ObtenerTodos() => _repository.DameTodos();

    public IList<LineaPrestamo> ObtenerPorPrestamo(long prestamoId) =>
        _repository.DamePorPrestamo(prestamoId);

    public void ActualizarDiasEstimados(long id, int dias)
    {
        var linea = _repository.DamePorOID(id)
            ?? throw new InvalidOperationException($"LineaPrestamo con id {id} no encontrada.");

        linea.DiasEstimados = dias;
        _repository.Modify(linea);
    }
}
