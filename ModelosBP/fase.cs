using System;
using System.Collections.Generic;

namespace ProyDesaWeb2025.ModelosBP;

public partial class fase
{
    public int Id_Fase { get; set; }

    public string Descripcion { get; set; } = null!;

    public int? Estado { get; set; }

    public virtual ICollection<fases_articulo> fases_articulos { get; set; } = new List<fases_articulo>();

    public virtual ICollection<ordenesdetalle_fase> ordenesdetalle_fases { get; set; } = new List<ordenesdetalle_fase>();
}
