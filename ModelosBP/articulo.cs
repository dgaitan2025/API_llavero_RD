using System;
using System.Collections.Generic;

namespace ProyDesaWeb2025.ModelosBP;

public partial class articulo
{
    public int Id_Articulo { get; set; }

    public string Descripcion { get; set; } = null!;

    public int Costo { get; set; }

    public int Precio { get; set; }

    public sbyte Estado { get; set; }

    public virtual ICollection<fases_articulo> fases_articulos { get; set; } = new List<fases_articulo>();
}
