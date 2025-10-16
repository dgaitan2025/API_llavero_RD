using System;
using System.Collections.Generic;

namespace ProyDesaWeb2025.ModelosBP;

public partial class debug_log
{
    public int id { get; set; }

    public DateTime? fecha { get; set; }

    public string? proceso { get; set; }

    public string? mensaje { get; set; }

    public string? valor { get; set; }
}
