using HelpDesk.Api.Data;
using HelpDesk.Api.DTOs;
using HelpDesk.Api.Models;
using HelpDesk.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Api.Services;

public class ChamadoService
{
    private readonly AppDbContext _context;

    public ChamadoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ChamadoResponse>> ObterTodosAsync(int? usuarioId = null, int? tecnicoId = null)
    {
        var query = _context.Chamados
            .Include(c => c.Usuario)
            .Include(c => c.Tecnico)
            .Include(c => c.Categoria)
            .AsQueryable();

        if (usuarioId.HasValue)
            query = query.Where(c => c.UsuarioId == usuarioId.Value);

        if (tecnicoId.HasValue)
            query = query.Where(c => c.TecnicoId == tecnicoId.Value);

        return await query
            .OrderByDescending(c => c.DataCriacao)
            .Select(c => new ChamadoResponse
            {
                Id = c.Id,
                Titulo = c.Titulo,
                Descricao = c.Descricao,
                Prioridade = c.Prioridade.ToString(),
                Status = c.Status.ToString(),
                DataCriacao = c.DataCriacao,
                DataAtualizacao = c.DataAtualizacao,
                DataResolucao = c.DataResolucao,
                UsuarioId = c.UsuarioId,
                NomeUsuario = c.Usuario != null ? c.Usuario.Nome : null,
                TecnicoId = c.TecnicoId,
                NomeTecnico = c.Tecnico != null ? c.Tecnico.Nome : null,
                CategoriaId = c.CategoriaId,
                NomeCategoria = c.Categoria != null ? c.Categoria.Nome : null
            })
            .ToListAsync();
    }

    public async Task<ChamadoResponse?> ObterPorIdAsync(int id)
    {
        return await _context.Chamados
            .Include(c => c.Usuario)
            .Include(c => c.Tecnico)
            .Include(c => c.Categoria)
            .Where(c => c.Id == id)
            .Select(c => new ChamadoResponse
            {
                Id = c.Id,
                Titulo = c.Titulo,
                Descricao = c.Descricao,
                Prioridade = c.Prioridade.ToString(),
                Status = c.Status.ToString(),
                DataCriacao = c.DataCriacao,
                DataAtualizacao = c.DataAtualizacao,
                DataResolucao = c.DataResolucao,
                UsuarioId = c.UsuarioId,
                NomeUsuario = c.Usuario != null ? c.Usuario.Nome : null,
                TecnicoId = c.TecnicoId,
                NomeTecnico = c.Tecnico != null ? c.Tecnico.Nome : null,
                CategoriaId = c.CategoriaId,
                NomeCategoria = c.Categoria != null ? c.Categoria.Nome : null
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ChamadoResponse> CriarAsync(int usuarioId, CriarChamadoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Titulo))
            throw new InvalidOperationException("O título do chamado é obrigatório.");

        if (string.IsNullOrWhiteSpace(request.Descricao))
            throw new InvalidOperationException("A descrição do chamado é obrigatória.");

        if (request.CategoriaId <= 0)
            throw new InvalidOperationException("A categoria é obrigatória.");

        var categoriaExiste = await _context.Categorias.AnyAsync(c => c.Id == request.CategoriaId);
        if (!categoriaExiste)
            throw new InvalidOperationException("Categoria não encontrada.");

        if (!Enum.TryParse<PrioridadeChamado>(request.Prioridade, true, out var prioridade))
            throw new InvalidOperationException("Prioridade inválida.");

        var chamado = new Chamado
        {
            Titulo = request.Titulo,
            Descricao = request.Descricao,
            Prioridade = prioridade,
            Status = StatusChamado.Aberto,
            UsuarioId = usuarioId,
            CategoriaId = request.CategoriaId,
            DataCriacao = DateTime.UtcNow
        };

        _context.Chamados.Add(chamado);
        await _context.SaveChangesAsync();

        return await ObterPorIdAsync(chamado.Id) ?? throw new InvalidOperationException("Erro ao recuperar o chamado criado.");
    }

    public async Task<ChamadoResponse> AtualizarAsync(int id, int usuarioLogadoId, string perfilUsuario, AtualizarChamadoRequest request)
    {
        var chamado = await _context.Chamados
            .Include(c => c.Usuario)
            .Include(c => c.Tecnico)
            .Include(c => c.Categoria)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (chamado is null)
            throw new KeyNotFoundException("Chamado não encontrado.");

        if (!string.IsNullOrWhiteSpace(request.Titulo))
            chamado.Titulo = request.Titulo;

        if (!string.IsNullOrWhiteSpace(request.Descricao))
            chamado.Descricao = request.Descricao;

        if (!string.IsNullOrWhiteSpace(request.Prioridade) && Enum.TryParse<PrioridadeChamado>(request.Prioridade, true, out var prioridade))
            chamado.Prioridade = prioridade;

        if (request.CategoriaId.HasValue)
        {
            var categoriaExiste = await _context.Categorias.AnyAsync(c => c.Id == request.CategoriaId.Value);
            if (!categoriaExiste)
                throw new InvalidOperationException("Categoria não encontrada.");

            chamado.CategoriaId = request.CategoriaId.Value;
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (!Enum.TryParse<StatusChamado>(request.Status, true, out var novoStatus))
                throw new InvalidOperationException("Status inválido.");

            ValidarTransicaoStatus(chamado.Status, novoStatus);
            chamado.Status = novoStatus;

            if (novoStatus == StatusChamado.Resolvido)
                chamado.DataResolucao ??= DateTime.UtcNow;

            if (novoStatus == StatusChamado.Fechado)
                chamado.DataAtualizacao = DateTime.UtcNow;
        }

        if (request.TecnicoId.HasValue)
        {
            if (perfilUsuario != PerfilUsuario.Administrador.ToString() && perfilUsuario != PerfilUsuario.Tecnico.ToString())
                throw new InvalidOperationException("Você não tem permissão para atribuir técnico.");

            var tecnicoExiste = await _context.Usuarios.AnyAsync(u => u.Id == request.TecnicoId.Value && u.Perfil == PerfilUsuario.Tecnico);
            if (!tecnicoExiste)
                throw new InvalidOperationException("Técnico não encontrado ou inválido.");

            chamado.TecnicoId = request.TecnicoId.Value;
        }

        chamado.DataAtualizacao = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new ChamadoResponse
        {
            Id = chamado.Id,
            Titulo = chamado.Titulo,
            Descricao = chamado.Descricao,
            Prioridade = chamado.Prioridade.ToString(),
            Status = chamado.Status.ToString(),
            DataCriacao = chamado.DataCriacao,
            DataAtualizacao = chamado.DataAtualizacao,
            DataResolucao = chamado.DataResolucao,
            UsuarioId = chamado.UsuarioId,
            NomeUsuario = chamado.Usuario?.Nome,
            TecnicoId = chamado.TecnicoId,
            NomeTecnico = chamado.Tecnico?.Nome,
            CategoriaId = chamado.CategoriaId,
            NomeCategoria = chamado.Categoria?.Nome
        };
    }

    public async Task RemoverAsync(int id)
    {
        var chamado = await _context.Chamados.FindAsync(id);
        if (chamado is null)
            throw new KeyNotFoundException("Chamado não encontrado.");

        _context.Chamados.Remove(chamado);
        await _context.SaveChangesAsync();
    }

    private static void ValidarTransicaoStatus(StatusChamado statusAtual, StatusChamado novoStatus)
    {
        var transicoesValidas = new Dictionary<StatusChamado, List<StatusChamado>>
        {
            { StatusChamado.Aberto, new() { StatusChamado.EmAtendimento } },
            { StatusChamado.EmAtendimento, new() { StatusChamado.AguardandoUsuario, StatusChamado.Resolvido } },
            { StatusChamado.AguardandoUsuario, new() { StatusChamado.EmAtendimento } },
            { StatusChamado.Resolvido, new() { StatusChamado.Fechado } },
            { StatusChamado.Fechado, new() }
        };

        if (statusAtual == novoStatus)
            return;

        if (!transicoesValidas.TryGetValue(statusAtual, out var validos) || !validos.Contains(novoStatus))
            throw new InvalidOperationException("Transição de status inválida.");
    }
}
