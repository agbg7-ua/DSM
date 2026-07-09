using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using ApplicationCore.Domain.Repositories;
using ApplicationCore.Domain.Security;

namespace ApplicationCore.Domain.CEN;

public class UsuarioCEN
{
    private readonly IUsuarioRepository _repository;
    private readonly IPrestamoRepository _prestamoRepository;
    private readonly IMaterialRepository _materialRepository;

    public UsuarioCEN(IUsuarioRepository repository, IPrestamoRepository prestamoRepository, IMaterialRepository materialRepository)
    {
        _repository = repository;
        _prestamoRepository = prestamoRepository;
        _materialRepository = materialRepository;
    }

    public long Crear(string nombre, string email, string contrasenia, RolUsuario rol)
    {
        var usuario = new Usuario
        {
            Nombre = nombre,
            Email = email,
            Contrasenia = PasswordHasher.Hash(contrasenia),
            Rol = rol
        };
        return _repository.New(usuario);
    }

    public void Modificar(long id, string nombre, string email, string? nuevaContrasenia, RolUsuario rol)
    {
        var usuario = _repository.DamePorOID(id)
            ?? throw new InvalidOperationException($"Usuario con id {id} no encontrado.");

        usuario.Nombre = nombre;
        usuario.Email = email;
        if (!string.IsNullOrWhiteSpace(nuevaContrasenia))
        {
            usuario.Contrasenia = PasswordHasher.Hash(nuevaContrasenia);
        }
        usuario.Rol = rol;
        _repository.Modify(usuario);
    }

    public void Eliminar(long id)
    {
        var usuario = _repository.DamePorOID(id)
            ?? throw new InvalidOperationException($"Usuario con id {id} no encontrado.");

        // Se eliminan primero los préstamos (y sus líneas) del usuario de forma explícita.
        // Importante: se filtra por la propiedad de navegación "Usuario" (la que está mapeada
        // por NHibernate), no por el campo "UsuarioId" de la entidad, que es una propiedad
        // "sombra" que el ORM nunca rellena y siempre valdría 0.
        var prestamos = _prestamoRepository.DameTodos()
            .Where(p => p.Usuario != null && p.Usuario.Id == id)
            .ToList();

        foreach (var prestamo in prestamos)
        {
            _prestamoRepository.Destroy(prestamo);
        }

        // Los materiales pueden tener asignado un usuario responsable (Material.UsuarioAsignado).
        // Esa relación no tiene cascade, así que hay que desasignarla explícitamente o la
        // base de datos rechaza el borrado del usuario por clave foránea. Igual que arriba,
        // se filtra por la navegación "UsuarioAsignado", no por el "UsuarioId" sin mapear.
        var materialesAsignados = _materialRepository.DameTodos()
            .Where(m => m.UsuarioAsignado != null && m.UsuarioAsignado.Id == id)
            .ToList();

        foreach (var material in materialesAsignados)
        {
            material.UsuarioAsignado = null;
            _materialRepository.Modify(material);
        }

        _repository.Destroy(usuario);
    }

    public bool VerificarContrasenia(long id, string contrasenia)
    {
        var usuario = _repository.DamePorOID(id);
        if (usuario == null || string.IsNullOrWhiteSpace(contrasenia))
        {
            return false;
        }
        return PasswordHasher.Verify(contrasenia, usuario.Contrasenia);
    }

    public bool EsCuentaExterna(long id)
    {
        var usuario = _repository.DamePorOID(id);
        return usuario != null && !string.IsNullOrWhiteSpace(usuario.ProveedorExterno);
    }

    public Usuario? ObtenerPorId(long id) => _repository.DamePorOID(id);

    public IList<Usuario> ObtenerTodos() => _repository.DameTodos();

    public bool Login(string email, string contrasenia)
    {
        var usuario = _repository.DameTodos().FirstOrDefault(u => u.Email == email);
        return usuario != null && PasswordHasher.Verify(contrasenia, usuario.Contrasenia);
    }

    public Usuario? ObtenerPorIdExterno(string proveedor, string idExterno) =>
        _repository.DameTodos()
            .FirstOrDefault(u => u.ProveedorExterno == proveedor && u.IdExterno == idExterno);

    public Usuario? ObtenerPorEmail(string email) =>
        _repository.DameTodos().FirstOrDefault(u => u.Email == email);

    public long CrearExterno(string nombre, string email, string proveedor, string idExterno)
    {
        var usuario = new Usuario
        {
            Nombre = nombre,
            Email = email,

            Contrasenia = "!external-login!" + Guid.NewGuid(),
            Rol = RolUsuario.Usuario,
            ProveedorExterno = proveedor,
            IdExterno = idExterno
        };
        return _repository.New(usuario);
    }

    public void VincularExterno(long id, string proveedor, string idExterno)
    {
        var usuario = _repository.DamePorOID(id)
            ?? throw new InvalidOperationException($"Usuario con id {id} no encontrado.");
        usuario.ProveedorExterno = proveedor;
        usuario.IdExterno = idExterno;
        _repository.Modify(usuario);
    }
}
