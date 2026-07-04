// "Copyright (c) YOUR_COMPANY. All rights reserved."

using System.ComponentModel.DataAnnotations;
using ApplicationCore.Domain.Enums;

namespace WebMarkerSpace.Models {
    public class UsuarioViewModel
    {
        [Display(Name = "Identificador")]
        public long Id { get; set; }

        [Display(Name = "Nombre de Usuario")]
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        public string Nombre { get; set; }

        [Display(Name = "Correo Electrónico")]
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Correo inválido.")]
        public string Email { get; set; }

        [Display(Name = "Contraseña")]
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        public string Contrasenia { get; set; }

        [Display(Name = "Rol de Usuario")]
        [Required(ErrorMessage = "El rol es obligatorio.")]
        public RolUsuario Rol { get; set; }
    }
    public class LoginUsuarioViewModel {
        [Display(Name = "Correo Electrónico")]
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Correo inválido.")]
        public string Email { get; set; }

        [Display(Name = "Contraseña")]
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        public string Contrasenia { get; set; }
    }

    // Modelo del formulario de registro público. Deliberadamente NO tiene
    // propiedad "Rol": cualquier usuario que se registra por su cuenta
    // recibe siempre el rol "Usuario" (se fuerza en el controlador), nunca
    // se confía en un valor de rol que venga del cliente.
    public class RegistroUsuarioViewModel {
        [Display(Name = "Nombre de Usuario")]
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        public string Nombre { get; set; }

        [Display(Name = "Correo Electrónico")]
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Correo inválido.")]
        public string Email { get; set; }

        [Display(Name = "Contraseña")]
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        public string Contrasenia { get; set; }

        [Display(Name = "Confirmar contraseña")]
        [Required(ErrorMessage = "Debes confirmar la contraseña.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Contrasenia), ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmarContrasenia { get; set; }
    }
}
