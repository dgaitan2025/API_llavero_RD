using System;
using System.Collections.Generic;

namespace ProyDesaWeb2025.ModelosBP;

public partial class role
{
    public byte RolId { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<usuario> usuarios { get; set; } = new List<usuario>();
}
