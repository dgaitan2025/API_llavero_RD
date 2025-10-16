using System;
using System.Collections.Generic;

namespace ProyDesaWeb2025.ModelosBP;

public partial class usuario
{
    public ulong UsuarioId { get; set; }

    public string email { get; set; } = null!;

    public string? Telefono { get; set; }

    public DateOnly? FechaNacimiento { get; set; }

    public string nickname { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public byte[]? Fotografia { get; set; }

    public string? FotografiaMime { get; set; }

    public byte[]? Fotografia2 { get; set; }

    public string? Fotografia2Mime { get; set; }

    public byte RolId { get; set; }

    public bool? EstaActivo { get; set; }

    public virtual role Rol { get; set; } = null!;
}
