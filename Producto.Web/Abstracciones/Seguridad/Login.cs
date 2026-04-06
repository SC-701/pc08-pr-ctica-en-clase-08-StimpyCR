// NUEVO: Abstracciones/Modelos/Seguridad/Login.cs
using System.ComponentModel.DataAnnotations;

namespace Abstracciones.Modelos.Seguridad
{
    public class LoginBase
    {
        public string? NombreUsuario { get; set; }
        public string? PasswordHash { get; set; }
        [Required(ErrorMessage = "El correo es requerido")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string CorreoElectronico { get; set; }
    }

    public class Login : LoginBase
    {
        [Required]
        public Guid Id { get; set; }
    }

    public class LoginRequest : LoginBase
    {
        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mínimo 8 caracteres")]
        public string Password { get; set; }

    }
}