namespace ProyDesaWeb2025.Models;

public class UsuarioCreateForm
{
    // Campos obligatorios del SP
    public string Email { get; set; } = "";
    public string Nickname { get; set; } = "";

    // Password plano (se hashea antes de guardar)
    public string PasswordPlano { get; set; } = "";

    // Opcionales
    public string? Telefono { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public byte RolId { get; set; }  // TINYINT UNSIGNED

    // Fotos en base64 + mimeType
    public string? FotografiaBase64 { get; set; }
    public string? FotografiaMime { get; set; }

    public string? Fotografia2Base64 { get; set; }
    public string? Fotografia2Mime { get; set; }
}
