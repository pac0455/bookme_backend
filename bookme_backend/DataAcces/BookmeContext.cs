using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using bookme_backend.DataAcces.Models;

namespace bookme_backend.DataAcces.Models;

public partial class BookmeContext : IdentityDbContext<Usuario>
{
    public BookmeContext()
    {
    }

    public BookmeContext(DbContextOptions<BookmeContext> options) : base(options)
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurar restricción para evitar múltiples caminos en cascada
        modelBuilder.Entity<Valoracione>()
            .HasOne(v => v.Reserva)
            .WithMany(r => r.Valoraciones)
            .HasForeignKey(v => v.ReservaId)
            .OnDelete(DeleteBehavior.Restrict); // ⚠️ EVITA el error DE MULTIPLES CASCADE APUNTANDO A UNA TABLA

        modelBuilder.Entity<Valoracione>()
            .HasOne(v => v.Usuario)
            .WithMany(u => u.Valoraciones)
            .HasForeignKey(v => v.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict); //  EVITA el error DE MULTIPLES CASCADE APUNTANDO A UNA TABLA

        modelBuilder.Entity<ReservasServicio>()
       .HasOne(rs => rs.Reserva)
       .WithMany(r => r.ReservasServicios)
       .HasForeignKey(rs => rs.ReservaId)
       .OnDelete(DeleteBehavior.Cascade); // ✅

        //ESTO SE RESOLVERA CON TRIGGER
        modelBuilder.Entity<ReservasServicio>()
            .HasOne(rs => rs.Servicio)
            .WithMany(s => s.ReservasServicios)
            .HasForeignKey(rs => rs.ServicioId)
            .OnDelete(DeleteBehavior.Restrict); // También evita conflicto
    }

public DbSet<bookme_backend.DataAcces.Models.Horarios> Horarios { get; set; } = default!;


}
