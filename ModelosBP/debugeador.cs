using System;
using System.Collections.Generic;

namespace ProyDesaWeb2025.ModelosBP;

public partial class debugeador
{
    public int Id { get; set; }

    public string Mensaje { get; set; } = null!;

    public DateTime? FechaRegistro { get; set; }
}
