using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HelpDesk.Api.Data;
using HelpDesk.Api.DTOs;
using HelpDesk.Api.Models;
using HelpDesk.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace HelpDesk.Api.Services;

public class AuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
            throw new InvalidOperationException("O nome é obrigatório.");

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new InvalidOperationException("O email é obrigatório.");

        if (string.IsNullOrWhiteSpace(request.Senha) || request.Senha.Length < 6)
            throw new InvalidOperationException("A senha deve ter no mínimo 6 caracteres.");

        var emailJaExiste = await _context.Usuarios.AnyAsync(u => u.Email == request.Email);
        if (emailJaExiste)
            throw new InvalidOperationException("Já existe um usuário com este email.");

        var usuario = new Usuario
        {
            Nome = request.Nome,
            Email = request.Email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha),
            Perfil = PerfilUsuario.Usuario,
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        return GerarTokenAsync(usuario);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new InvalidOperationException("O email é obrigatório.");

        if (string.IsNullOrWhiteSpace(request.Senha))
            throw new InvalidOperationException("A senha é obrigatória.");

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (usuario is null || !usuario.Ativo)
            throw new InvalidOperationException("Credenciais inválidas.");

        var senhaValida = BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash);
        if (!senhaValida)
            throw new InvalidOperationException("Credenciais inválidas.");

        return GerarTokenAsync(usuario);
    }

    private AuthResponse GerarTokenAsync(Usuario usuario)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? "helpdesk-secret-key-super-segura-123";
        var issuer = _configuration["Jwt:Issuer"] ?? "HelpDeskApi";
        var audience = _configuration["Jwt:Audience"] ?? "HelpDeskApi";

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, usuario.Perfil.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiracao = DateTime.UtcNow.AddHours(8);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiracao,
            signingCredentials: creds
        );

        return new AuthResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiraEm = expiracao,
            Nome = usuario.Nome,
            Email = usuario.Email,
            Perfil = usuario.Perfil.ToString()
        };
    }
}
