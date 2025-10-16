using System;
using System.Collections.Generic;

namespace ProyDesaWeb2025.ModelosBP;

public partial class ordenes_detalle
{
    public int Id_Detalle { get; set; }

    public int Id_Orden { get; set; }

    public int Id_Articulo { get; set; }

    public int Cantidad { get; set; }

    public int Precio { get; set; }

    public int Subtotal { get; set; }

    public byte[]? Foto_Anverso { get; set; }

    public byte[]? Foto_Reverso { get; set; }

    public string? Link { get; set; }

    public string? Texto { get; set; }

    public int? Id_Tipo_Grabado { get; set; }

    public string? Nombre { get; set; }

    public int? Telefono { get; set; }

    public virtual ordene Id_OrdenNavigation { get; set; } = null!;

    public virtual tipo_grabados_nfc? Id_Tipo_GrabadoNavigation { get; set; }

    public virtual ICollection<ordenesdetalle_fase> ordenesdetalle_fases { get; set; } = new List<ordenesdetalle_fase>();
}
