// "Copyright (c) YOUR_COMPANY. All rights reserved."

using System.ComponentModel.DataAnnotations;
using ApplicationCore.Domain.Enums;

namespace WebMarkerSpace.Models {
    public class UsuarioViewModel
    {
        [Display(Name = "Common.Field.Id")]
        public long Id { get; set; }

        [Display(Name = "Common.Field.Username")]
        [Required(ErrorMessage = "Common.Field.Username.Required")]
        public string Nombre { get; set; }

        [Display(Name = "Common.Field.Email")]
        [Required(ErrorMessage = "Common.Field.Email.Required")]
        [EmailAddress(ErrorMessage = "Common.Field.Email.Invalid")]
        public string Email { get; set; }

        [Display(Name = "Common.Field.Password")]
        [Required(ErrorMessage = "Common.Field.Password.Required")]
        [DataType(DataType.Password)]
        public string Contrasenia { get; set; }

        [Display(Name = "Common.Field.Role")]
        [Required(ErrorMessage = "Common.Field.Role.Required")]
        public RolUsuario Rol { get; set; }
    }
    public class LoginUsuarioViewModel {
        [Display(Name = "Common.Field.Email")]
        [Required(ErrorMessage = "Common.Field.Email.Required")]
        [EmailAddress(ErrorMessage = "Common.Field.Email.Invalid")]
        public string Email { get; set; }

        [Display(Name = "Common.Field.Password")]
        [Required(ErrorMessage = "Common.Field.Password.Required")]
        [DataType(DataType.Password)]
        public string Contrasenia { get; set; }
    }

    // Modelo del formulario de registro público. Deliberadamente NO tiene
    // propiedad "Rol": cualquier usuario que se registra por su cuenta
    // recibe siempre el rol "Usuario" (se fuerza en el controlador), nunca
    // se confía en un valor de rol que venga del cliente.
    public class RegistroUsuarioViewModel {
        [Display(Name = "Common.Field.Username")]
        [Required(ErrorMessage = "Common.Field.Username.Required")]
        public string Nombre { get; set; }

        [Display(Name = "Common.Field.Email")]
        [Required(ErrorMessage = "Common.Field.Email.Required")]
        [EmailAddress(ErrorMessage = "Common.Field.Email.Invalid")]
        public string Email { get; set; }

        [Display(Name = "Common.Field.Password")]
        [Required(ErrorMessage = "Common.Field.Password.Required")]
        [DataType(DataType.Password)]
        public string Contrasenia { get; set; }

        [Display(Name = "Common.Field.ConfirmPassword")]
        [Required(ErrorMessage = "Common.Field.ConfirmPassword.Required")]
        [DataType(DataType.Password)]
        [Compare(nameof(Contrasenia), ErrorMessage = "Common.Field.ConfirmPassword.Mismatch")]
        public string ConfirmarContrasenia { get; set; }
    }

    // Modelo del formulario de "Mi perfil": el propio usuario edita su nombre,
    // email y (opcionalmente) su contraseña. Deliberadamente NO tiene "Rol":
    // nadie puede ascenderse a sí mismo a Administrador desde aquí.
    public class PerfilViewModel {
        [Display(Name = "Common.Field.Id")]
        public long Id { get; set; }

        [Display(Name = "Common.Field.Username")]
        [Required(ErrorMessage = "Common.Field.Username.Required")]
        public string Nombre { get; set; }

        [Display(Name = "Common.Field.Email")]
        [Required(ErrorMessage = "Common.Field.Email.Required")]
        [EmailAddress(ErrorMessage = "Common.Field.Email.Invalid")]
        public string Email { get; set; }

        [Display(Name = "Common.Field.Role")]
        public RolUsuario Rol { get; set; }

        [Display(Name = "Common.Field.NewPassword")]
        [DataType(DataType.Password)]
        public string? NuevaContrasenia { get; set; }

        [Display(Name = "Common.Field.ConfirmNewPassword")]
        [DataType(DataType.Password)]
        [Compare(nameof(NuevaContrasenia), ErrorMessage = "Common.Field.ConfirmPassword.Mismatch")]
        public string? ConfirmarNuevaContrasenia { get; set; }
    }
}
