namespace HelpDesk.Api.DTOs;

public class AtualizarUsuarioRequest
{
    public string? Nome { get; set; }
    public string? Email { get; set; }
    public string? Perfil { get; set; }
    public bool? Ativo { get; set; }
}
