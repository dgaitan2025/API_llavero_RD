namespace ProyDesaWeb2025.Models.Dto;

public class LoginRequest
{
    public string usuario { get; set; } = string.Empty; // Email, Teléfono o Nickname
    public string password { get; set; } = string.Empty;
}