namespace HelpDesk.Api.Models;

public class Comentario
{
    public int Id { get; set; }
    public string Texto { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public int ChamadoId { get; set; }
    public Chamado? Chamado { get; set; }
}
