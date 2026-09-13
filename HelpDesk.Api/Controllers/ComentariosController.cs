using System.Security.Claims;
using HelpDesk.Api.DTOs;
using HelpDesk.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Controllers;

[ApiController]
[Route("api/chamados/{chamadoId:int}/comentarios")]
[Authorize]
public class ComentariosController : ControllerBase
{
    private readonly ComentarioService _comentarioService;
    private readonly ChamadoService _chamadoService;

    public ComentariosController(ComentarioService comentarioService, ChamadoService chamadoService)
    {
        _comentarioService = comentarioService;
        _chamadoService = chamadoService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ComentarioResponse>>> GetAll(int chamadoId)
    {
        var chamado = await _chamadoService.ObterPorIdAsync(chamadoId);
        if (chamado is null)
            return NotFound();

        var perfil = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (perfil != "Administrador" && chamado.UsuarioId != usuarioId && chamado.TecnicoId != usuarioId)
            return Forbid();

        var comentarios = await _comentarioService.ObterPorChamadoAsync(chamadoId);
        return Ok(comentarios);
    }

    [HttpPost]
    public async Task<ActionResult<ComentarioResponse>> Create(int chamadoId, [FromBody] CriarComentarioRequest request)
    {
        try
        {
            var chamado = await _chamadoService.ObterPorIdAsync(chamadoId);
            if (chamado is null)
                return NotFound();

            var perfil = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (perfil != "Administrador" && chamado.UsuarioId != usuarioId && chamado.TecnicoId != usuarioId)
                return Forbid();

            var comentario = await _comentarioService.CriarAsync(chamadoId, usuarioId, request);
            return CreatedAtAction(nameof(GetAll), new { chamadoId }, comentario);
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

    [HttpPut("{comentarioId:int}")]
    public async Task<ActionResult<ComentarioResponse>> Update(int chamadoId, int comentarioId, [FromBody] AtualizarComentarioRequest request)
    {
        try
        {
            var chamado = await _chamadoService.ObterPorIdAsync(chamadoId);
            if (chamado is null)
                return NotFound();

            var perfil = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (perfil != "Administrador" && chamado.UsuarioId != usuarioId && chamado.TecnicoId != usuarioId)
                return Forbid();

            var comentario = await _comentarioService.AtualizarAsync(chamadoId, comentarioId, usuarioId, perfil, request);
            return Ok(comentario);
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

    [HttpDelete("{comentarioId:int}")]
    public async Task<IActionResult> Delete(int chamadoId, int comentarioId)
    {
        try
        {
            var chamado = await _chamadoService.ObterPorIdAsync(chamadoId);
            if (chamado is null)
                return NotFound();

            var perfil = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (perfil != "Administrador" && chamado.UsuarioId != usuarioId && chamado.TecnicoId != usuarioId)
                return Forbid();

            await _comentarioService.RemoverAsync(chamadoId, comentarioId, usuarioId, perfil);
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
