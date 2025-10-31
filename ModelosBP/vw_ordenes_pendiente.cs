using System;
using System.Collections.Generic;

namespace ProyDesaWeb2025.ModelosBP;

public partial class vw_ordenes_pendiente
{
    public int Id_Orden { get; set; }
    public int entrega_domicilio { get; set; }

    public int Id_Detalle { get; set; }
    public int fase_actual { get; set; }
    public byte[]? Foto_Anverso { get; set; }

    public byte[]? Foto_Reverso { get; set; }

    public string? Tipo_Grabado { get; set; }

    public string? Link { get; set; }

    public string? Nombre { get; set; }

    public long? Telefono { get; set; }

    public int Fill1 { get; set; }
    public int Fill2 { get; set; }
    public int Cantidad { get; set; }
}
