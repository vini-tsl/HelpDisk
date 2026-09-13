namespace HelpDesk.Api.Models;

public class Categoria
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }

    public ICollection<Chamado> Chamados { get; set; } = new List<Chamado>();
}
