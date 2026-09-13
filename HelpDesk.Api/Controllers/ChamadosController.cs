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
public class ChamadosController : ControllerBase
{
    private readonly ChamadoService _chamadoService;

    public ChamadosController(ChamadoService chamadoService)
    {
        _chamadoService = chamadoService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ChamadoResponse>>> GetAll()
    {
        var perfil = User.FindFirstValue(ClaimTypes.Role);
        var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (perfil == PerfilUsuario.Administrador.ToString())
            return Ok(await _chamadoService.ObterTodosAsync());

        if (perfil == PerfilUsuario.Tecnico.ToString())
            return Ok(await _chamadoService.ObterTodosAsync(tecnicoId: usuarioId));

        return Ok(await _chamadoService.ObterTodosAsync(usuarioId: usuarioId));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ChamadoResponse>> GetById(int id)
    {
        var chamado = await _chamadoService.ObterPorIdAsync(id);
        if (chamado is null)
            return NotFound();

        var perfil = User.FindFirstValue(ClaimTypes.Role);
        var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (perfil == PerfilUsuario.Administrador.ToString())
            return Ok(chamado);

        if (perfil == PerfilUsuario.Tecnico.ToString() && chamado.TecnicoId == usuarioId)
            return Ok(chamado);

        if (perfil == PerfilUsuario.Usuario.ToString() && chamado.UsuarioId == usuarioId)
            return Ok(chamado);

        return Forbid();
    }

    [HttpPost]
    public async Task<ActionResult<ChamadoResponse>> Create([FromBody] CriarChamadoRequest request)
    {
        try
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var chamado = await _chamadoService.CriarAsync(usuarioId, request);
            return CreatedAtAction(nameof(GetById), new { id = chamado.Id }, chamado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ChamadoResponse>> Update(int id, [FromBody] AtualizarChamadoRequest request)
    {
        try
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var perfil = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
            var chamado = await _chamadoService.AtualizarAsync(id, usuarioId, perfil, request);
            return Ok(chamado);
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

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _chamadoService.RemoverAsync(id);
            return NoContent();
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
