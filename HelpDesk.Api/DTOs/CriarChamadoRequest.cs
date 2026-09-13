using HelpDesk.Api.Models.Enums;

namespace HelpDesk.Api.DTOs;

public class CriarChamadoRequest
{
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Prioridade { get; set; } = "Media";
    public int CategoriaId { get; set; }
}
