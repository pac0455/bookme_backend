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

    public virtual DbSet<Suscripcion> Suscripciones { get; set; }

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
            .OnDelete(DeleteBehavior.Restrict); //  EVITA el error DE MULTIPLES CASCADE APUNTANDO A UNA TABLA

        modelBuilder.Entity<Valoracione>()
            .HasOne(v => v.Usuario)
            .WithMany(u => u.Valoraciones)
            .HasForeignKey(v => v.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict); //  EVITA el error DE MULTIPLES CASCADE APUNTANDO A UNA TABLA

        modelBuilder.Entity<ReservasServicio>()
       .HasOne(rs => rs.Reserva)
       .WithMany(r => r.ReservasServicios)
       .HasForeignKey(rs => rs.ReservaId)
       .OnDelete(DeleteBehavior.Cascade); // 

        //ESTO SE RESOLVERA CON TRIGGER
        modelBuilder.Entity<ReservasServicio>()
            .HasOne(rs => rs.Servicio)
            .WithMany(s => s.ReservasServicios)
            .HasForeignKey(rs => rs.ServicioId)
            .OnDelete(DeleteBehavior.Restrict); // También evita conflicto

        //Cuando se elimine un Negocio, elimina también sus HorariosAtencion asociados
        modelBuilder.Entity<Horario>()
            .HasOne(h => h.Negocio)
            .WithMany(n => n.HorariosAtencion)
            .HasForeignKey(h => h.IdNegocio)
            .OnDelete(DeleteBehavior.Cascade);
        //Seed para añadir categorias por defecto
        modelBuilder.Entity<Categoria>().HasData(
            new Categoria { Id = 1, Nombre = "Sin categoría" },
            new Categoria { Id = 2, Nombre = "Gimnasio" },
            new Categoria { Id = 3, Nombre = "Salón de belleza" },
            new Categoria { Id = 4, Nombre = "Barbería" },
            new Categoria { Id = 5, Nombre = "Clínica dental" },
            new Categoria { Id = 6, Nombre = "Spa" },
            new Categoria { Id = 7, Nombre = "Fisioterapia" },
            new Categoria { Id = 8, Nombre = "Psicología" },
            new Categoria { Id = 9, Nombre = "Nutrición" },
            new Categoria { Id = 10, Nombre = "Entrenador personal" },
            new Categoria { Id = 11, Nombre = "Veterinaria" },
            new Categoria { Id = 12, Nombre = "Tatuajes" }
        );


    }

    public DbSet<Horario> Horarios { get; set; } = default!;
    public DbSet<Categoria> Categorias { get; set; }


}
