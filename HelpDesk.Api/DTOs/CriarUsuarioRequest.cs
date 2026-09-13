namespace HelpDesk.Api.DTOs;

public class CriarUsuarioRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string Perfil { get; set; } = "Usuario";
    public bool Ativo { get; set; } = true;
}
