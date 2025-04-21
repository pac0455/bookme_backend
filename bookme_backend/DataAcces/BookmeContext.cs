using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace bookme_backend.DataAcces.Models;

public partial class BookmeContext : DbContext
{
    public BookmeContext()
    {
    }

    public BookmeContext(DbContextOptions<BookmeContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Negocio> Negocios { get; set; }

    public virtual DbSet<Pago> Pagos { get; set; }

    public virtual DbSet<Reserva> Reservas { get; set; }

    public virtual DbSet<ReservasServicio> ReservasServicios { get; set; }

    public virtual DbSet<Servicio> Servicios { get; set; }

    public virtual DbSet<Suscripcione> Suscripciones { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Valoracione> Valoraciones { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost,1433;Database=Bookme;User=sa;Password=S3cur3P@ss2024!;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Negocio>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__negocios__3213E83F2DFB86ED");
        });

        modelBuilder.Entity<Pago>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__pagos__3213E83FA26CBEB4");

            entity.Property(e => e.Creado).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.FechaPago).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Moneda).HasDefaultValue("EUR");
            entity.Property(e => e.Reembolsado).HasDefaultValue(false);

            entity.HasOne(d => d.Reserva).WithMany(p => p.Pagos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__pagos__reserva_i__5535A963");
        });

        modelBuilder.Entity<Reserva>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__reservas__3213E83FAD634A78");

            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Negocio).WithMany(p => p.Reservas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__reservas__negoci__4E88ABD4");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Reservas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__reservas__usuari__4F7CD00D");
        });

        modelBuilder.Entity<ReservasServicio>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__reservas__3213E83FC9E3188A");

            entity.HasOne(d => d.Reserva).WithMany(p => p.ReservasServicios)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__reservas___reser__534D60F1");

            entity.HasOne(d => d.Servicio).WithMany(p => p.ReservasServicios)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__reservas___servi__5441852A");
        });

        modelBuilder.Entity<Servicio>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__servicio__3213E83FC409B076");

            entity.HasOne(d => d.Negocio).WithMany(p => p.Servicios)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__servicios__negoc__52593CB8");
        });

        modelBuilder.Entity<Suscripcione>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__suscripc__3213E83F04BFEB90");

            entity.HasOne(d => d.IdNegocioNavigation).WithMany(p => p.Suscripciones)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__suscripci__id_ne__4CA06362");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Suscripciones)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__suscripci__id_us__4D94879B");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__usuarios__3213E83FBAA18244");

            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<Valoracione>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__valoraci__3213E83F8BC8F58B");

            entity.Property(e => e.FechaValoracion).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Reserva).WithMany(p => p.Valoraciones)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__valoracio__reser__5070F446");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Valoraciones)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__valoracio__usuar__5165187F");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
