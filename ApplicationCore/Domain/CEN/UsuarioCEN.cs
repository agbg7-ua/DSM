using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN;

public class UsuarioCEN
{
    private readonly IUsuarioRepository _repository;

    public UsuarioCEN(IUsuarioRepository repository)
    {
        _repository = repository;
    }

    public long Crear(string nombre, string email, string contrasenia, RolUsuario rol)
    {
        var usuario = new Usuario
        {
            Nombre = nombre,
            Email = email,
            Contrasenia = contrasenia,
            Rol = rol
        };
        return _repository.New(usuario);
    }

    public void Modificar(long id, string nombre, string email, string contrasenia, RolUsuario rol)
    {
        var usuario = _repository.DamePorOID(id)
            ?? throw new InvalidOperationException($"Usuario con id {id} no encontrado.");

        usuario.Nombre = nombre;
        usuario.Email = email;
        usuario.Contrasenia = contrasenia;
        usuario.Rol = rol;
        _repository.Modify(usuario);
    }

    public void Eliminar(long id)
    {
        var usuario = _repository.DamePorOID(id)
            ?? throw new InvalidOperationException($"Usuario con id {id} no encontrado.");
        _repository.Destroy(usuario);
    }

    public Usuario? ObtenerPorId(long id) => _repository.DamePorOID(id);

    public IList<Usuario> ObtenerTodos() => _repository.DameTodos();

    public bool Login(string email, string contrasenia)
    {
        return _repository.DameTodos()
            .Any(u => u.Email == email && u.Contrasenia == contrasenia);
    }

    // --- Soporte de login federado (OAuth2 / OpenID Connect) ---

    /// <summary>
    /// Busca un usuario ya vinculado a un proveedor externo concreto por su
    /// identificador estable (claim "sub"). Es la forma preferente y más segura
    /// de reconocer a un usuario recurrente, porque no depende del email.
    /// </summary>
    public Usuario? ObtenerPorIdExterno(string proveedor, string idExterno) =>
        _repository.DameTodos()
            .FirstOrDefault(u => u.ProveedorExterno == proveedor && u.IdExterno == idExterno);

    public Usuario? ObtenerPorEmail(string email) =>
        _repository.DameTodos().FirstOrDefault(u => u.Email == email);

    /// <summary>
    /// Aprovisionamiento "just-in-time": crea una cuenta local para un usuario que
    /// se ha autenticado correctamente en el proveedor externo pero que todavía no
    /// existe en nuestro sistema. Nunca se le asigna el rol Administrador desde aquí.
    /// </summary>
    public long CrearExterno(string nombre, string email, string proveedor, string idExterno)
    {
        var usuario = new Usuario
        {
            Nombre = nombre,
            Email = email,
            // Las cuentas federadas no tienen contraseña local utilizable: se guarda
            // un valor no comparable para que Login(email, contrasenia) nunca casee.
            Contrasenia = "!external-login!" + Guid.NewGuid(),
            Rol = RolUsuario.Usuario,
            ProveedorExterno = proveedor,
            IdExterno = idExterno
        };
        return _repository.New(usuario);
    }

    /// <summary>
    /// Vincula una cuenta local ya existente (creada con email/contraseña) con un
    /// proveedor externo, para que en próximos accesos se reconozca por IdExterno.
    /// </summary>
    public void VincularExterno(long id, string proveedor, string idExterno)
    {
        var usuario = _repository.DamePorOID(id)
            ?? throw new InvalidOperationException($"Usuario con id {id} no encontrado.");
        usuario.ProveedorExterno = proveedor;
        usuario.IdExterno = idExterno;
        _repository.Modify(usuario);
    }
}
