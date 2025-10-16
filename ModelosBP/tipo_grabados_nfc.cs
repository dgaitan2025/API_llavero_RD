using System;
using System.Collections.Generic;

namespace ProyDesaWeb2025.ModelosBP;

public partial class tipo_grabados_nfc
{
    public int Id_Tipo_Grabado { get; set; }

    public string? Descripcion { get; set; }

    public sbyte? estado { get; set; }

    public virtual ICollection<ordenes_detalle> ordenes_detalles { get; set; } = new List<ordenes_detalle>();
}
