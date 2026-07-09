
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
        [DataType(DataType.Password)]
        public string Contrasenia { get; set; }
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

        public bool EsCuentaExterna { get; set; }
    }
}
