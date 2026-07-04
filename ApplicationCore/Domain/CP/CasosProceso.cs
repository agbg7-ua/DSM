using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CP;

public class CasosProceso
{
    private readonly PrestamoCEN _prestamoCEN;
    private readonly LineaPrestamoCEN _lineaPrestamoCEN;
    private readonly MaterialCEN _materialCEN;
    private readonly UsuarioCEN _usuarioCEN;
    private readonly IUnitOfWork _unitOfWork;

    public CasosProceso(
        PrestamoCEN prestamoCEN,
        LineaPrestamoCEN lineaPrestamoCEN,
        MaterialCEN materialCEN,
        UsuarioCEN usuarioCEN,
        IUnitOfWork unitOfWork)
    {
        _prestamoCEN = prestamoCEN;
        _lineaPrestamoCEN = lineaPrestamoCEN;
        _materialCEN = materialCEN;
        _usuarioCEN = usuarioCEN;
        _unitOfWork = unitOfWork;
    }

    public long SolicitarPrestamo(long usuarioId, long[] materialIds)
    {
        if (materialIds.Length == 0)
            throw new InvalidOperationException("Debe indicar al menos un material.");

        if (_usuarioCEN.ObtenerPorId(usuarioId) is null)
            throw new InvalidOperationException($"Usuario con id {usuarioId} no encontrado.");

        var materiales = new List<(Material Material, int DiasEstimados)>();
        foreach (var materialId in materialIds)
        {
            var material = _materialCEN.ObtenerPorId(materialId)
                ?? throw new InvalidOperationException($"Material con id {materialId} no encontrado.");

            if (!material.EstaDisponible || material.Estado != EstadoMaterial.Disponible)
                throw new InvalidOperationException($"Material con id {materialId} no está disponible.");

            materiales.Add((material, 7));
        }

        var totalDias = materiales.Sum(m => m.DiasEstimados);
        var prestamoId = _prestamoCEN.Crear(
            usuarioId,
            DateTime.UtcNow,
            EstadoPrestamo.Activo,
            totalDias);

        foreach (var (material, diasEstimados) in materiales)
        {
            _lineaPrestamoCEN.Crear(prestamoId, material.Id, diasEstimados);
            _materialCEN.Modificar(
                material.Id,
                material.Nombre,
                material.Descripcion,
                EstadoMaterial.Disponible,
                false,
                material.Imagen, // <-- Corregido: se pasa el string Imagen como sexto argumento
                usuarioId);
        }

        _unitOfWork.SaveChanges();
        return prestamoId;
    }

    public void DevolverMaterial(long prestamoId)
    {
        var prestamo = _prestamoCEN.ObtenerPorId(prestamoId)
            ?? throw new InvalidOperationException($"Prestamo con id {prestamoId} no encontrado.");

        if (prestamo.Estado == EstadoPrestamo.Devuelto)
            throw new InvalidOperationException($"Prestamo con id {prestamoId} ya fue devuelto.");

        foreach (var linea in _lineaPrestamoCEN.ObtenerPorPrestamo(prestamoId))
        {
            var material = _materialCEN.ObtenerPorId(linea.Material.Id)
                ?? throw new InvalidOperationException($"Material con id {linea.Material.Id} no encontrado.");

            _materialCEN.Modificar(
                material.Id,
                material.Nombre,
                material.Descripcion,
                EstadoMaterial.Disponible,
                true,
                material.Imagen, // <-- Corregido: se pasa el string Imagen como sexto argumento
                null);
        }

        _prestamoCEN.Modificar(
            prestamoId,
            prestamo.Usuario.Id,
            prestamo.FechaCreacion,
            EstadoPrestamo.Devuelto,
            prestamo.TotalDias);

        _unitOfWork.SaveChanges();
    }
}
