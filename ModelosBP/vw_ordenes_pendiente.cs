using System;
using System.Collections.Generic;

namespace ProyDesaWeb2025.ModelosBP;

public partial class vw_ordenes_pendiente
{
    public int Id_Orden { get; set; }
    public int entrega_domicilio { get; set; }

    public int Id_Detalle { get; set; }
    public byte[]? Foto_Anverso { get; set; }

    public byte[]? Foto_Reverso { get; set; }

    public string? Tipo_Grabado { get; set; }

    public string? Link { get; set; }

    public string? Nombre { get; set; }

    public long? Telefono { get; set; }
}
