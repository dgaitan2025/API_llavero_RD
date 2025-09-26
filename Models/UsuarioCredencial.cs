namespace ProyDesaWeb2025.Models;

public class UsuarioCredencial
{
    public ulong UsuarioId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public byte[]? Fotografia { get; set; }   // login (en BD)
    public string? FotografiaMime { get; set; }   // Base64 para login
    public byte[]? Fotografia2 { get; set; }  // personalizada (en BD)
    public string? Fotografia2Mime { get; set; }   // Base64 para login
    public string PasswordHash { get; set; } = string.Empty;
    public byte RolId { get; set; }
    public bool EstaActivo { get; set; }
}