namespace HelpDesk.Api.DTOs;

public class CriarCategoriaRequest
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
}
