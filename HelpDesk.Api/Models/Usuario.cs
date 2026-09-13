using HelpDesk.Api.Models.Enums;

namespace HelpDesk.Api.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public PerfilUsuario Perfil { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public ICollection<Chamado> ChamadosAbertos { get; set; } = new List<Chamado>();
    public ICollection<Chamado> ChamadosAtendidos { get; set; } = new List<Chamado>();
    public ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();
}
