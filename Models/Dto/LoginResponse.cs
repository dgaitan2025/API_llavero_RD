namespace ProyDesaWeb2025.Models.Dto;

public class LoginResponse
{
    public ulong UsuarioId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public string? Fotografia { get; set; }   // Base64 para login
    public string? Fotografia2 { get; set; }  // Base64 personalizada
    public byte RolId { get; set; }
    public bool EstaActivo { get; set; }
    public string? Token { get; set; } // JWT
}