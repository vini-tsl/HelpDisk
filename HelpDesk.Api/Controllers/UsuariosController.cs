using System.Security.Claims;
using HelpDesk.Api.DTOs;
using HelpDesk.Api.Models.Enums;
using HelpDesk.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsuariosController : ControllerBase
{
    private readonly UsuarioService _usuarioService;

    public UsuariosController(UsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [HttpGet]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<List<UsuarioResponse>>> GetAll()
    {
        var usuarios = await _usuarioService.ObterTodosAsync();
        return Ok(usuarios);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UsuarioResponse>> GetById(int id)
    {
        var usuarioLogadoId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var perfil = User.FindFirstValue(ClaimTypes.Role);

        if (!User.IsInRole(PerfilUsuario.Administrador.ToString()) && usuarioLogadoId != id)
        {
            return Forbid();
        }

        var usuario = await _usuarioService.ObterPorIdAsync(id);
        if (usuario is null)
            return NotFound();

        return Ok(usuario);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<UsuarioResponse>> Create([FromBody] CriarUsuarioRequest request)
    {
        try
        {
            var usuario = await _usuarioService.CriarAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = usuario.Id }, usuario);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UsuarioResponse>> Update(int id, [FromBody] AtualizarUsuarioRequest request)
    {
        var usuarioLogadoId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var perfil = User.FindFirstValue(ClaimTypes.Role);

        if (!User.IsInRole(PerfilUsuario.Administrador.ToString()) && usuarioLogadoId != id)
        {
            return Forbid();
        }

        try
        {
            var usuario = await _usuarioService.AtualizarAsync(id, request);
            return Ok(usuario);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
