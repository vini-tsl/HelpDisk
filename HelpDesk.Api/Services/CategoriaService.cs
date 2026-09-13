using HelpDesk.Api.Data;
using HelpDesk.Api.DTOs;
using HelpDesk.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Api.Services;

public class CategoriaService
{
    private readonly AppDbContext _context;

    public CategoriaService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoriaResponse>> ObterTodasAsync()
    {
        return await _context.Categorias
            .OrderBy(c => c.Nome)
            .Select(c => new CategoriaResponse
            {
                Id = c.Id,
                Nome = c.Nome,
                Descricao = c.Descricao
            })
            .ToListAsync();
    }

    public async Task<CategoriaResponse?> ObterPorIdAsync(int id)
    {
        return await _context.Categorias
            .Where(c => c.Id == id)
            .Select(c => new CategoriaResponse
            {
                Id = c.Id,
                Nome = c.Nome,
                Descricao = c.Descricao
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CategoriaResponse> CriarAsync(CriarCategoriaRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
            throw new InvalidOperationException("O nome da categoria é obrigatório.");

        if (await _context.Categorias.AnyAsync(c => c.Nome == request.Nome))
            throw new InvalidOperationException("Já existe uma categoria com este nome.");

        var categoria = new Categoria
        {
            Nome = request.Nome,
            Descricao = request.Descricao
        };

        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();

        return new CategoriaResponse
        {
            Id = categoria.Id,
            Nome = categoria.Nome,
            Descricao = categoria.Descricao
        };
    }

    public async Task<CategoriaResponse> AtualizarAsync(int id, AtualizarCategoriaRequest request)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria is null)
            throw new KeyNotFoundException("Categoria não encontrada.");

        if (!string.IsNullOrWhiteSpace(request.Nome))
        {
            if (await _context.Categorias.AnyAsync(c => c.Nome == request.Nome && c.Id != id))
                throw new InvalidOperationException("Já existe outra categoria com este nome.");

            categoria.Nome = request.Nome;
        }

        if (request.Descricao is not null)
            categoria.Descricao = request.Descricao;

        await _context.SaveChangesAsync();

        return new CategoriaResponse
        {
            Id = categoria.Id,
            Nome = categoria.Nome,
            Descricao = categoria.Descricao
        };
    }

    public async Task RemoverAsync(int id)
    {
        var categoria = await _context.Categorias
            .Include(c => c.Chamados)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (categoria is null)
            throw new KeyNotFoundException("Categoria não encontrada.");

        if (categoria.Chamados.Any())
            throw new InvalidOperationException("Não é possível remover uma categoria que está sendo usada em chamados.");

        _context.Categorias.Remove(categoria);
        await _context.SaveChangesAsync();
    }
}
