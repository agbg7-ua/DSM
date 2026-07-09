using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using ApplicationCore.Domain.Repositories;
using ApplicationCore.Domain.Security;

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
        _repository.Destroy(usuario);
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
