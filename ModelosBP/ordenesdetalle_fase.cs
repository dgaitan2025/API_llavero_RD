using System;
using System.Collections.Generic;

namespace ProyDesaWeb2025.ModelosBP;

public partial class ordenesdetalle_fase
{
    public long Id_Registro { get; set; }

    public int Id_Detalle { get; set; }

    public int Id_Fase { get; set; }

    public DateTime Fecha_Inicio { get; set; }

    public DateTime? Fecha_Fin { get; set; }

    public string? Comentario { get; set; }

    public virtual ordenes_detalle Id_DetalleNavigation { get; set; } = null!;

    public virtual fase Id_FaseNavigation { get; set; } = null!;
}
