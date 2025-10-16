using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ProyDesaWeb2025.ModelosBP;

public partial class DBDesWeb : DbContext
{
    public DBDesWeb(DbContextOptions<DBDesWeb> options)
        : base(options)
    {
    }

    public virtual DbSet<articulo> articulos { get; set; }

    public virtual DbSet<debug_log> debug_logs { get; set; }

    public virtual DbSet<debugeador> debugeadors { get; set; }

    public virtual DbSet<fase> fases { get; set; }

    public virtual DbSet<fases_articulo> fases_articulos { get; set; }

    public virtual DbSet<metodos_pago> metodos_pagos { get; set; }

    public virtual DbSet<ordene> ordenes { get; set; }

    public virtual DbSet<ordenes_detalle> ordenes_detalles { get; set; }

    public virtual DbSet<ordenesdetalle_fase> ordenesdetalle_fases { get; set; }

    public virtual DbSet<role> roles { get; set; }

    public virtual DbSet<tipo_grabados_nfc> tipo_grabados_nfcs { get; set; }

    public virtual DbSet<usuario> usuarios { get; set; }

    public virtual DbSet<vw_ordenes_finalizada> vw_ordenes_finalizadas { get; set; }

    public virtual DbSet<vw_ordenes_pendiente> vw_ordenes_pendientes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<articulo>(entity =>
        {
            entity.HasKey(e => e.Id_Articulo).HasName("PRIMARY");

            entity.Property(e => e.Descripcion).HasMaxLength(150);
        });

        modelBuilder.Entity<debug_log>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.ToTable("debug_log");

            entity.Property(e => e.fecha)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.mensaje).HasColumnType("text");
            entity.Property(e => e.proceso).HasMaxLength(100);
            entity.Property(e => e.valor).HasColumnType("text");
        });

        modelBuilder.Entity<debugeador>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("debugeador");

            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.Mensaje).HasMaxLength(500);
        });

        modelBuilder.Entity<fase>(entity =>
        {
            entity.HasKey(e => e.Id_Fase).HasName("PRIMARY");

            entity.Property(e => e.Descripcion).HasMaxLength(255);
            entity.Property(e => e.Estado).HasDefaultValueSql("'1'");
        });

        modelBuilder.Entity<fases_articulo>(entity =>
        {
            entity.HasKey(e => e.Id_Registro).HasName("PRIMARY");

            entity.HasIndex(e => e.Id_Articulo, "Id_Articulo");

            entity.HasIndex(e => e.Id_Fase, "Id_Fase");

            entity.HasOne(d => d.Id_ArticuloNavigation).WithMany(p => p.fases_articulos)
                .HasForeignKey(d => d.Id_Articulo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fases_articulos_ibfk_2");

            entity.HasOne(d => d.Id_FaseNavigation).WithMany(p => p.fases_articulos)
                .HasForeignKey(d => d.Id_Fase)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fases_articulos_ibfk_1");
        });

        modelBuilder.Entity<metodos_pago>(entity =>
        {
            entity.HasKey(e => e.Id_Metodo).HasName("PRIMARY");

            entity.ToTable("metodos_pago");

            entity.Property(e => e.Descripcion).HasMaxLength(150);
        });

        modelBuilder.Entity<ordene>(entity =>
        {
            entity.HasKey(e => e.Id_Orden).HasName("PRIMARY");

            entity.HasIndex(e => e.Id_Tipo_Pago, "ordenes_id_tipo_pago_foreign");

            entity.Property(e => e.Direccion_entrega).HasMaxLength(255);
            entity.Property(e => e.Persona_Entregar).HasMaxLength(150);
            entity.Property(e => e.Telefono).HasMaxLength(20);
            entity.Property(e => e.entrega_domicilio)
                .HasDefaultValueSql("b'0'")
                .HasColumnType("bit(1)");

            entity.HasOne(d => d.Id_Tipo_PagoNavigation).WithMany(p => p.ordenes)
                .HasForeignKey(d => d.Id_Tipo_Pago)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ordenes_id_tipo_pago_foreign");
        });

        modelBuilder.Entity<ordenes_detalle>(entity =>
        {
            entity.HasKey(e => e.Id_Detalle).HasName("PRIMARY");

            entity.ToTable("ordenes_detalle");

            entity.HasIndex(e => e.Id_Tipo_Grabado, "fk_tipo_grabado");

            entity.HasIndex(e => e.Id_Orden, "ordenes_detalle_id_orden_foreign");

            entity.Property(e => e.Link).HasMaxLength(255);
            entity.Property(e => e.Nombre).HasMaxLength(150);
            entity.Property(e => e.Texto).HasMaxLength(255);

            entity.HasOne(d => d.Id_OrdenNavigation).WithMany(p => p.ordenes_detalles)
                .HasForeignKey(d => d.Id_Orden)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ordenes_detalle_id_orden_foreign");

            entity.HasOne(d => d.Id_Tipo_GrabadoNavigation).WithMany(p => p.ordenes_detalles)
                .HasForeignKey(d => d.Id_Tipo_Grabado)
                .HasConstraintName("fk_tipo_grabado");
        });

        modelBuilder.Entity<ordenesdetalle_fase>(entity =>
        {
            entity.HasKey(e => e.Id_Registro).HasName("PRIMARY");

            entity.HasIndex(e => e.Id_Detalle, "ordenesdetalle_fases_id_detalle_foreign");

            entity.HasIndex(e => e.Id_Fase, "ordenesdetalle_fases_id_fase_foreign");

            entity.Property(e => e.Comentario).HasMaxLength(255);
            entity.Property(e => e.Fecha_Fin).HasColumnType("datetime");
            entity.Property(e => e.Fecha_Inicio).HasColumnType("datetime");

            entity.HasOne(d => d.Id_DetalleNavigation).WithMany(p => p.ordenesdetalle_fases)
                .HasForeignKey(d => d.Id_Detalle)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ordenesdetalle_fases_id_detalle_foreign");

            entity.HasOne(d => d.Id_FaseNavigation).WithMany(p => p.ordenesdetalle_fases)
                .HasForeignKey(d => d.Id_Fase)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ordenesdetalle_fases_id_fase_foreign");
        });

        modelBuilder.Entity<role>(entity =>
        {
            entity.HasKey(e => e.RolId).HasName("PRIMARY");

            entity
                .HasCharSet("latin1")
                .UseCollation("latin1_swedish_ci");

            entity.HasIndex(e => e.Nombre, "Nombre").IsUnique();

            entity.Property(e => e.Nombre).HasMaxLength(32);
        });

        modelBuilder.Entity<tipo_grabados_nfc>(entity =>
        {
            entity.HasKey(e => e.Id_Tipo_Grabado).HasName("PRIMARY");

            entity.ToTable("tipo_grabados_nfc");

            entity.Property(e => e.Descripcion).HasMaxLength(100);
            entity.Property(e => e.estado).HasDefaultValueSql("'1'");
        });

        modelBuilder.Entity<usuario>(entity =>
        {
            entity.HasKey(e => e.UsuarioId).HasName("PRIMARY");

            entity
                .HasCharSet("latin1")
                .UseCollation("latin1_swedish_ci");

            entity.HasIndex(e => e.EstaActivo, "idx_usuarios_activo");

            entity.HasIndex(e => e.email, "idx_usuarios_email").IsUnique();

            entity.HasIndex(e => e.nickname, "idx_usuarios_nickname").IsUnique();

            entity.HasIndex(e => e.RolId, "idx_usuarios_rol");

            entity.HasIndex(e => e.Telefono, "idx_usuarios_telefono").IsUnique();

            entity.Property(e => e.EstaActivo)
                .IsRequired()
                .HasDefaultValueSql("'1'");
            entity.Property(e => e.Fotografia).HasColumnType("mediumblob");
            entity.Property(e => e.Fotografia2).HasColumnType("mediumblob");
            entity.Property(e => e.Fotografia2Mime).HasMaxLength(64);
            entity.Property(e => e.FotografiaMime).HasMaxLength(64);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Telefono).HasMaxLength(32);

            entity.HasOne(d => d.Rol).WithMany(p => p.usuarios)
                .HasForeignKey(d => d.RolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_usuarios_roles");
        });

        modelBuilder.Entity<vw_ordenes_finalizada>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_ordenes_finalizadas");

            entity.Property(e => e.Link).HasMaxLength(255);
            entity.Property(e => e.Nombre).HasMaxLength(150);
            entity.Property(e => e.Texto).HasMaxLength(255);
            entity.Property(e => e.Tipo_grabado).HasMaxLength(100);
        });

        modelBuilder.Entity<vw_ordenes_pendiente>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_ordenes_pendientes");

            entity.Property(e => e.Link).HasMaxLength(255);
            entity.Property(e => e.Nombre).HasMaxLength(150);
            entity.Property(e => e.Tipo_Grabado).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
