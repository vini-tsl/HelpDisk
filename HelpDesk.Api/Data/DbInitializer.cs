using HelpDesk.Api.Models;
using HelpDesk.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Api.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await context.Database.MigrateAsync();

        if (await context.Usuarios.AnyAsync())
            return;

        var categorias = new[]
        {
            new Categoria { Nome = "Infraestrutura", Descricao = "Problemas ligados a rede, hardware e ambiente de trabalho." },
            new Categoria { Nome = "Aplicativo", Descricao = "Erros e melhorias em sistemas internos e aplicações web." },
            new Categoria { Nome = "Conta de Usuário", Descricao = "Acesso, autenticação, permissões e credenciais." }
        };

        context.Categorias.AddRange(categorias);
        await context.SaveChangesAsync();

        var admin = new Usuario
        {
            Nome = "Administrador",
            Email = "admin@helpdesk.com",
            SenhaHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Perfil = PerfilUsuario.Administrador,
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        };

        var tecnico = new Usuario
        {
            Nome = "Técnico Suporte",
            Email = "tecnico@helpdesk.com",
            SenhaHash = BCrypt.Net.BCrypt.HashPassword("tecnico123"),
            Perfil = PerfilUsuario.Tecnico,
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        };

        var usuario = new Usuario
        {
            Nome = "Usuário Comum",
            Email = "usuario@helpdesk.com",
            SenhaHash = BCrypt.Net.BCrypt.HashPassword("usuario123"),
            Perfil = PerfilUsuario.Usuario,
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        };

        context.Usuarios.AddRange(admin, tecnico, usuario);
        await context.SaveChangesAsync();

        var chamado = new Chamado
        {
            Titulo = "VPN não está conectando",
            Descricao = "O usuário relatou que a VPN está recusando a conexão após a atualização do cliente.",
            Prioridade = PrioridadeChamado.Alta,
            Status = StatusChamado.Aberto,
            UsuarioId = usuario.Id,
            TecnicoId = tecnico.Id,
            CategoriaId = categorias[0].Id,
            DataCriacao = DateTime.UtcNow
        };

        context.Chamados.Add(chamado);
        await context.SaveChangesAsync();

        context.Comentarios.Add(new Comentario
        {
            Texto = "Solicitação registrada e encaminhada para análise do suporte.",
            ChamadoId = chamado.Id,
            UsuarioId = usuario.Id,
            DataCriacao = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
    }
}
