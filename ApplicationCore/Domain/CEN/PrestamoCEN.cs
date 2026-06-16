using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN;

public class PrestamoCEN
{
    private readonly IPrestamoRepository _repository;
    private readonly IUsuarioRepository _usuarioRepository;

    public PrestamoCEN(IPrestamoRepository repository, IUsuarioRepository usuarioRepository)
    {
        _repository = repository;
        _usuarioRepository = usuarioRepository;
    }

    public long Crear(long usuarioId, DateTime fechaCreacion, EstadoPrestamo estado, int totalDias)
    {
        var usuario = _usuarioRepository.DamePorOID(usuarioId)
            ?? throw new InvalidOperationException($"Usuario con id {usuarioId} no encontrado.");

        var prestamo = new Prestamo
        {
            Usuario = usuario,
            FechaCreacion = fechaCreacion,
            Estado = estado,
            TotalDias = totalDias
        };
        return _repository.New(prestamo);
    }

    public void Modificar(long id, long usuarioId, DateTime fechaCreacion, EstadoPrestamo estado, int totalDias)
    {
        var prestamo = _repository.DamePorOID(id)
            ?? throw new InvalidOperationException($"Prestamo con id {id} no encontrado.");

        var usuario = _usuarioRepository.DamePorOID(usuarioId)
            ?? throw new InvalidOperationException($"Usuario con id {usuarioId} no encontrado.");

        prestamo.Usuario = usuario;
        prestamo.FechaCreacion = fechaCreacion;
        prestamo.Estado = estado;
        prestamo.TotalDias = totalDias;
        _repository.Modify(prestamo);
    }

    public void Eliminar(long id)
    {
        var prestamo = _repository.DamePorOID(id)
            ?? throw new InvalidOperationException($"Prestamo con id {id} no encontrado.");
        _repository.Destroy(prestamo);
    }

    public Prestamo? ObtenerPorId(long id) => _repository.DamePorOID(id);

    public IList<Prestamo> ObtenerTodos() => _repository.DameTodos();

    public IList<Prestamo> ObtenerActivosPorUsuario(long usuarioId) =>
        _repository.DameFilterActivosPorUsuario(usuarioId);

    public IList<Prestamo> ObtenerRetrasados() => _repository.DameFilterRetrasados();
}
