using HelpDesk.Api.Models.Enums;

namespace HelpDesk.Api.DTOs;

public class ChamadoResponse
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Prioridade { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public DateTime? DataResolucao { get; set; }
    public int UsuarioId { get; set; }
    public string? NomeUsuario { get; set; }
    public int? TecnicoId { get; set; }
    public string? NomeTecnico { get; set; }
    public int CategoriaId { get; set; }
    public string? NomeCategoria { get; set; }
}
