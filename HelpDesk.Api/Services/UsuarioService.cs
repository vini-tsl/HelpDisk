using HelpDesk.Api.Data;
using HelpDesk.Api.DTOs;
using HelpDesk.Api.Models;
using HelpDesk.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Api.Services;

public class UsuarioService
{
    private readonly AppDbContext _context;

    public UsuarioService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<UsuarioResponse>> ObterTodosAsync()
    {
        return await _context.Usuarios
            .OrderBy(u => u.Nome)
            .Select(u => new UsuarioResponse
            {
                Id = u.Id,
                Nome = u.Nome,
                Email = u.Email,
                Perfil = u.Perfil.ToString(),
                Ativo = u.Ativo,
                DataCriacao = u.DataCriacao
            })
            .ToListAsync();
    }

    public async Task<UsuarioResponse?> ObterPorIdAsync(int id)
    {
        return await _context.Usuarios
            .Where(u => u.Id == id)
            .Select(u => new UsuarioResponse
            {
                Id = u.Id,
                Nome = u.Nome,
                Email = u.Email,
                Perfil = u.Perfil.ToString(),
                Ativo = u.Ativo,
                DataCriacao = u.DataCriacao
            })
            .FirstOrDefaultAsync();
    }

    public async Task<UsuarioResponse> CriarAsync(CriarUsuarioRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
            throw new InvalidOperationException("O nome é obrigatório.");

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new InvalidOperationException("O email é obrigatório.");

        if (string.IsNullOrWhiteSpace(request.Senha) || request.Senha.Length < 6)
            throw new InvalidOperationException("A senha deve ter no mínimo 6 caracteres.");

        if (await _context.Usuarios.AnyAsync(u => u.Email == request.Email))
            throw new InvalidOperationException("Já existe um usuário com este email.");

        if (!Enum.TryParse<PerfilUsuario>(request.Perfil, out var perfil))
            throw new InvalidOperationException("Perfil inválido.");

        var usuario = new Usuario
        {
            Nome = request.Nome,
            Email = request.Email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha),
            Perfil = perfil,
            Ativo = request.Ativo,
            DataCriacao = DateTime.UtcNow
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        return new UsuarioResponse
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            Perfil = usuario.Perfil.ToString(),
            Ativo = usuario.Ativo,
            DataCriacao = usuario.DataCriacao
        };
    }

    public async Task<UsuarioResponse> AtualizarAsync(int id, AtualizarUsuarioRequest request)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario is null)
            throw new KeyNotFoundException("Usuário não encontrado.");

        if (!string.IsNullOrWhiteSpace(request.Nome))
            usuario.Nome = request.Nome;

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var emailJaExiste = await _context.Usuarios.AnyAsync(u => u.Email == request.Email && u.Id != id);
            if (emailJaExiste)
                throw new InvalidOperationException("Já existe outro usuário com este email.");

            usuario.Email = request.Email;
        }

        if (!string.IsNullOrWhiteSpace(request.Perfil) && Enum.TryParse<PerfilUsuario>(request.Perfil, out var perfil))
            usuario.Perfil = perfil;

        if (request.Ativo.HasValue)
            usuario.Ativo = request.Ativo.Value;

        await _context.SaveChangesAsync();

        return new UsuarioResponse
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            Perfil = usuario.Perfil.ToString(),
            Ativo = usuario.Ativo,
            DataCriacao = usuario.DataCriacao
        };
    }
}
