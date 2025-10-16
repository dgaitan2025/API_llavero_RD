using System;
using System.Collections.Generic;

namespace ProyDesaWeb2025.ModelosBP;

public partial class metodos_pago
{
    public int Id_Metodo { get; set; }

    public string Descripcion { get; set; } = null!;

    public sbyte estado { get; set; }

    public virtual ICollection<ordene> ordenes { get; set; } = new List<ordene>();
}
