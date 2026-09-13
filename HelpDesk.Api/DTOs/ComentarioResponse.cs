namespace HelpDesk.Api.DTOs;

public class ComentarioResponse
{
    public int Id { get; set; }
    public int ChamadoId { get; set; }
    public int UsuarioId { get; set; }
    public string? NomeUsuario { get; set; }
    public string Texto { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; }
}
