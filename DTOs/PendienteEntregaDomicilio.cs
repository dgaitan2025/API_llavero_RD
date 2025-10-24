namespace ProyDesaWeb2025.DTOs
{
    public sealed class PendienteEntregaDomicilio
    {
        public int Id_Orden { get; set; }
        public string? FaseActual { get; set; }
        public long TotalFases { get; set; }         // <-- long (COUNT(*) = BIGINT)
        public long PasoActual { get; set; }         // <-- por seguridad, usa long
        public decimal Porcentaje { get; set; }
        public int Tipo_pago { get; set; }
        public int Id_detalle { get; set; }
        public int Pago_realizado { get; set; }

        public string? Direccion_entrega { get; set; }
        public string? Persona_Entregar { get; set; }
        public string? Telefono { get; set; }
        public int? UsuarioRepartidor { get; set; }
    }
}
