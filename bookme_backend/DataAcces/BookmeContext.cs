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

    public virtual DbSet<Valoracion> Valoraciones { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurar restricción para evitar múltiples caminos en cascada
        modelBuilder.Entity<Valoracion>()
            .HasOne(v => v.Reserva)
            .WithMany(r => r.Valoraciones)
            .HasForeignKey(v => v.ReservaId)
            .OnDelete(DeleteBehavior.Restrict); //  EVITA el error DE MULTIPLES CASCADE APUNTANDO A UNA TABLA

        modelBuilder.Entity<Valoracion>()
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


        modelBuilder.Entity<Horario>()
            .HasOne(h => h.Negocio)
            .WithMany(n => n.HorariosAtencion)
            .HasForeignKey(h => h.IdNegocio)
            .OnDelete(DeleteBehavior.Cascade);

        //Seed categoria
        modelBuilder.Entity<Categoria>().HasData(
            new Categoria { Id = 1, Nombre = "Clínica" },
            new Categoria { Id = 2, Nombre = "Tienda" },
            new Categoria { Id = 3, Nombre = "Gimnasio" },
            new Categoria { Id = 4, Nombre = "Salón de Belleza" },
            new Categoria { Id = 5, Nombre = "Veterinaria" },
            new Categoria { Id = 6, Nombre = "Restaurante" },
            new Categoria { Id = 7, Nombre = "Cafetería" },
            new Categoria { Id = 8, Nombre = "Barbería" },
            new Categoria { Id = 9, Nombre = "Psicología" },
            new Categoria { Id = 10, Nombre = "Nutrición" },
            new Categoria { Id = 11, Nombre = "Fisioterapia" },
            new Categoria { Id = 12, Nombre = "Podología" },
            new Categoria { Id = 13, Nombre = "Asesoría" },
            new Categoria { Id = 14, Nombre = "Consultoría" },
            new Categoria { Id = 15, Nombre = "Servicios Jurídicos" },
            new Categoria { Id = 16, Nombre = "Clases Particulares" },
            new Categoria { Id = 17, Nombre = "Academia de Idiomas" },
            new Categoria { Id = 18, Nombre = "Tatuajes y Piercings" },
            new Categoria { Id = 19, Nombre = "Centro Estético" },
            new Categoria { Id = 20, Nombre = "Terapias Alternativas" },
            new Categoria { Id = 21, Nombre = "Cuidado de Mascotas" },
            new Categoria { Id = 22, Nombre = "Mecánica" },
            new Categoria { Id = 23, Nombre = "Electricista" },
            new Categoria { Id = 24, Nombre = "Fontanero" },
            new Categoria { Id = 25, Nombre = "Fotografía" }
        );

    }

    public DbSet<Horario> Horarios { get; set; } = default!;


}
