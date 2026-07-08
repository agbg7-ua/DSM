using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN;

public class MaterialCEN
{
    private readonly IMaterialRepository _repository;
    private readonly IUsuarioRepository _usuarioRepository;

    public MaterialCEN(IMaterialRepository repository, IUsuarioRepository usuarioRepository)
    {
        _repository = repository;
        _usuarioRepository = usuarioRepository;
    }

    public long Crear(string nombre, string descripcion, EstadoMaterial estado, CategoriaMaterial categoria, string Imagen, long? usuarioId = null)
    {
        var material = new Material();
            ActualizarMaterial(
                material,
                nombre,
                descripcion,
                estado,
                categoria,
                Imagen,
                usuarioId);

        return _repository.New(material);
    }

    public void Modificar(long id, string nombre, string descripcion, EstadoMaterial estado, CategoriaMaterial categoria, string Imagen, long? usuarioId = null)
    {
        var material = _repository.DamePorOID(id)
            ?? throw new InvalidOperationException($"Material con id {id} no encontrado.");

        ActualizarMaterial(
            material,
            nombre,
            descripcion,
            estado,
            categoria,
            Imagen,
            usuarioId);
        _repository.Modify(material);
    }

    public void Eliminar(long id)
    {
        var material = _repository.DamePorOID(id)
            ?? throw new InvalidOperationException($"Material con id {id} no encontrado.");
        _repository.Destroy(material);
    }

    public Material? ObtenerPorId(long id) => _repository.DamePorOID(id);

    public IList<Material> ObtenerTodos() => _repository.DameTodos();

    public IList<Material> ObtenerPorEstado(EstadoMaterial estado) => _repository.DameFilterPorEstado(estado);

    public void MarcarComoRoto(long id)
    {
        var material = _repository.DamePorOID(id)
            ?? throw new InvalidOperationException($"Material con id {id} no encontrado.");

        material.Estado = EstadoMaterial.Roto;
        _repository.Modify(material);
    }

    public void MarcarComoMantenimiento(long id)
    {
        var material = _repository.DamePorOID(id)
            ?? throw new InvalidOperationException($"Material con id {id} no encontrado.");

        material.Estado = EstadoMaterial.EnMantenimiento;
        _repository.Modify(material);
    }

    private void ActualizarMaterial(
    Material material,
    string nombre,
    string descripcion,
    EstadoMaterial estado,
    CategoriaMaterial categoria,
    string imagen,
    long? usuarioId) {
        material.Nombre = nombre;
        material.Descripcion = descripcion;
        material.Estado = estado;
        material.Categoria = categoria;
        material.Imagen = imagen;

        material.UsuarioAsignado = usuarioId.HasValue
            ? _usuarioRepository.DamePorOID(usuarioId.Value)
                ?? throw new InvalidOperationException(
                    $"Usuario con id {usuarioId.Value} no encontrado.")
            : null;
    }
}
