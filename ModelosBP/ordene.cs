using System;
using System.Collections.Generic;

namespace ProyDesaWeb2025.ModelosBP;

public partial class ordene
{
    public int Id_Orden { get; set; }

    public long Id_Usuario { get; set; }

    public int Id_Tipo_Pago { get; set; }

    public DateOnly Fecha { get; set; }

    public int Total { get; set; }

    public string Persona_Entregar { get; set; } = null!;

    public string Direccion_entrega { get; set; } = null!;

    public string Telefono { get; set; } = null!;

    public sbyte estado { get; set; }

    public int? operador { get; set; }

    public ulong entrega_domicilio { get; set; }

    public virtual metodos_pago Id_Tipo_PagoNavigation { get; set; } = null!;

    public virtual ICollection<ordenes_detalle> ordenes_detalles { get; set; } = new List<ordenes_detalle>();
}
