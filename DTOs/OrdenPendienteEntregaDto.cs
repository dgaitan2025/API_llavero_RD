namespace ProyDesaWeb2025.DTOs
{
    public sealed class OrdenPendienteEntregaDto
    {
        public int Id_Orden { get; set; }
        public string? FaseActual { get; set; }
        public long TotalFases { get; set; }         // <-- long (COUNT(*) = BIGINT)
        public long PasoActual { get; set; }         // <-- por seguridad, usa long
        public decimal Porcentaje { get; set; }
        public int Tipo_pago { get; set; }
        public int Id_detalle { get; set; }
        public int Pago_realizado { get; set; }
    }
}
