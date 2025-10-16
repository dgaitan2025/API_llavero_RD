using System;
using System.Collections.Generic;

namespace ProyDesaWeb2025.ModelosBP;

public partial class vw_ordenes_finalizada
{
    public int Id_Orden { get; set; }

    public ulong UsuarioId { get; set; }

    public string? Tipo_grabado { get; set; }

    public byte[]? Foto_Anverso { get; set; }

    public string? Link { get; set; }

    public string? Texto { get; set; }

    public string? Nombre { get; set; }

    public int? Telefono { get; set; }
}
