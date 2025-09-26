namespace ProyDesaWeb2025.Models.Dto;

public class LoginRequest
{
	public string Identificador { get; set; } = string.Empty; // Email, Teléfono o Nickname
    public string Password { get; set; } = string.Empty;
}