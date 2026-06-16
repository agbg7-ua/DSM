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
}
