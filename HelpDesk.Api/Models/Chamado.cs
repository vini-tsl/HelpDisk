using HelpDesk.Api.Models.Enums;

namespace HelpDesk.Api.Models;

public class Chamado
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public PrioridadeChamado Prioridade { get; set; }
    public StatusChamado Status { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime? DataAtualizacao { get; set; }
    public DateTime? DataResolucao { get; set; }

    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public int? TecnicoId { get; set; }
    public Usuario? Tecnico { get; set; }

    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    public ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();
}
