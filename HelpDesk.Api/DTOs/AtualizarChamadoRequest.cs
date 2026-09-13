namespace HelpDesk.Api.DTOs;

public class AtualizarChamadoRequest
{
    public string? Titulo { get; set; }
    public string? Descricao { get; set; }
    public string? Prioridade { get; set; }
    public string? Status { get; set; }
    public int? TecnicoId { get; set; }
    public int? CategoriaId { get; set; }
}
