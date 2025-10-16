using System;
using System.Collections.Generic;

namespace ProyDesaWeb2025.ModelosBP;

public partial class fases_articulo
{
    public int Id_Registro { get; set; }

    public int Id_Fase { get; set; }

    public int Id_Articulo { get; set; }

    public int No_Paso { get; set; }

    public virtual articulo Id_ArticuloNavigation { get; set; } = null!;

    public virtual fase Id_FaseNavigation { get; set; } = null!;
}
