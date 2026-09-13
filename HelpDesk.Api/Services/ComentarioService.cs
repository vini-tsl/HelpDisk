using HelpDesk.Api.Data;
using HelpDesk.Api.DTOs;
using HelpDesk.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Api.Services;

public class ComentarioService
{
    private readonly AppDbContext _context;

    public ComentarioService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ComentarioResponse>> ObterPorChamadoAsync(int chamadoId)
    {
        var chamadoExiste = await _context.Chamados.AnyAsync(c => c.Id == chamadoId);
        if (!chamadoExiste)
            throw new KeyNotFoundException("Chamado não encontrado.");

        return await _context.Comentarios
            .Include(c => c.Usuario)
            .Where(c => c.ChamadoId == chamadoId)
            .OrderBy(c => c.DataCriacao)
            .Select(c => new ComentarioResponse
            {
                Id = c.Id,
                ChamadoId = c.ChamadoId,
                UsuarioId = c.UsuarioId,
                NomeUsuario = c.Usuario != null ? c.Usuario.Nome : null,
                Texto = c.Texto,
                DataCriacao = c.DataCriacao
            })
            .ToListAsync();
    }

    public async Task<ComentarioResponse> CriarAsync(int chamadoId, int usuarioId, CriarComentarioRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Texto))
            throw new InvalidOperationException("O texto do comentário é obrigatório.");

        var chamadoExiste = await _context.Chamados.AnyAsync(c => c.Id == chamadoId);
        if (!chamadoExiste)
            throw new KeyNotFoundException("Chamado não encontrado.");

        var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.Id == usuarioId);
        if (!usuarioExiste)
            throw new InvalidOperationException("Usuário não encontrado.");

        var comentario = new Comentario
        {
            Texto = request.Texto.Trim(),
            ChamadoId = chamadoId,
            UsuarioId = usuarioId,
            DataCriacao = DateTime.UtcNow
        };

        _context.Comentarios.Add(comentario);
        await _context.SaveChangesAsync();

        var comentarioSalvo = await _context.Comentarios
            .Include(c => c.Usuario)
            .Where(c => c.Id == comentario.Id)
            .Select(c => new ComentarioResponse
            {
                Id = c.Id,
                ChamadoId = c.ChamadoId,
                UsuarioId = c.UsuarioId,
                NomeUsuario = c.Usuario != null ? c.Usuario.Nome : null,
                Texto = c.Texto,
                DataCriacao = c.DataCriacao
            })
            .FirstOrDefaultAsync();

        return comentarioSalvo ?? throw new InvalidOperationException("Erro ao salvar o comentário.");
    }

    public async Task<ComentarioResponse> AtualizarAsync(int chamadoId, int comentarioId, int usuarioId, string perfil, AtualizarComentarioRequest request)
    {
        var comentario = await _context.Comentarios
            .Include(c => c.Chamado)
            .FirstOrDefaultAsync(c => c.Id == comentarioId && c.ChamadoId == chamadoId);

        if (comentario is null)
            throw new KeyNotFoundException("Comentário não encontrado.");

        var podeEditar = perfil == "Administrador" || comentario.UsuarioId == usuarioId || comentario.Chamado?.UsuarioId == usuarioId || comentario.Chamado?.TecnicoId == usuarioId;
        if (!podeEditar)
            throw new InvalidOperationException("Você não tem permissão para editar este comentário.");

        if (string.IsNullOrWhiteSpace(request.Texto))
            throw new InvalidOperationException("O texto do comentário é obrigatório.");

        comentario.Texto = request.Texto.Trim();
        await _context.SaveChangesAsync();

        var comentarioAtualizado = await _context.Comentarios
            .Include(c => c.Usuario)
            .Where(c => c.Id == comentario.Id)
            .Select(c => new ComentarioResponse
            {
                Id = c.Id,
                ChamadoId = c.ChamadoId,
                UsuarioId = c.UsuarioId,
                NomeUsuario = c.Usuario != null ? c.Usuario.Nome : null,
                Texto = c.Texto,
                DataCriacao = c.DataCriacao
            })
            .FirstAsync();

        return comentarioAtualizado;
    }

    public async Task RemoverAsync(int chamadoId, int comentarioId, int usuarioId, string perfil)
    {
        var comentario = await _context.Comentarios
            .Include(c => c.Chamado)
            .FirstOrDefaultAsync(c => c.Id == comentarioId && c.ChamadoId == chamadoId);

        if (comentario is null)
            throw new KeyNotFoundException("Comentário não encontrado.");

        var podeRemover = perfil == "Administrador" || comentario.UsuarioId == usuarioId || comentario.Chamado?.UsuarioId == usuarioId || comentario.Chamado?.TecnicoId == usuarioId;
        if (!podeRemover)
            throw new InvalidOperationException("Você não tem permissão para remover este comentário.");

        _context.Comentarios.Remove(comentario);
        await _context.SaveChangesAsync();
    }
}
