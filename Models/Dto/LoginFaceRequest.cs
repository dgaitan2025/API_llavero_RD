namespace ProyDesaWeb2025.Models.Dto;

public class LoginFaceRequest
{
    /// <summary>Email, teléfono o nickname</summary>
    public string Identificador { get; set; } = string.Empty;

    /// <summary>Foto del rostro (imagen)</summary>
    public IFormFile Foto { get; set; } = default!;
}