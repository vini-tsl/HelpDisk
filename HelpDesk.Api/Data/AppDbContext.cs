using HelpDesk.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Chamado> Chamados { get; set; }
    public DbSet<Comentario> Comentarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Nome).IsRequired().HasMaxLength(200);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(200);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.SenhaHash).IsRequired();
            entity.Property(u => u.Perfil).IsRequired();
            entity.Property(u => u.Ativo).HasDefaultValue(true);
            entity.Property(u => u.DataCriacao).HasDefaultValueSql("NOW()");
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Nome).IsRequired().HasMaxLength(150);
            entity.Property(c => c.Descricao).HasMaxLength(500);
        });

        modelBuilder.Entity<Chamado>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Titulo).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Descricao).IsRequired();
            entity.Property(c => c.Prioridade).IsRequired();
            entity.Property(c => c.Status).IsRequired();
            entity.Property(c => c.DataCriacao).HasDefaultValueSql("NOW()");

            entity.HasOne(c => c.Usuario)
                .WithMany(u => u.ChamadosAbertos)
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.Tecnico)
                .WithMany(u => u.ChamadosAtendidos)
                .HasForeignKey(c => c.TecnicoId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(c => c.Categoria)
                .WithMany(cat => cat.Chamados)
                .HasForeignKey(c => c.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Comentario>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Texto).IsRequired();
            entity.Property(c => c.DataCriacao).HasDefaultValueSql("NOW()");

            entity.HasOne(c => c.Usuario)
                .WithMany(u => u.Comentarios)
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.Chamado)
                .WithMany(ch => ch.Comentarios)
                .HasForeignKey(c => c.ChamadoId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
