using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyDesaWeb2025.ModelosBP.Dto
{
    public class OrdenesViewModel
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

        [Column(TypeName = "bit(1)")]
        public ulong entrega_domicilio { get; set; }
        public string detalles { get; set; } = null!;
    }
}
