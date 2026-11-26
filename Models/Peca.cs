using SQLite;
using System.Diagnostics.CodeAnalysis;

namespace CadastroClientes.Models;

public class Peca
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int OS { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string CodigoPeca { get; set; } = string.Empty;
    public int Qtde { get; set; }
    public decimal Valor { get; set; }
    public int Tecnico { get; set; }
    public DateTime DtaAprovacao { get; set; }
    public DateTime DtaReprovacao { get; set; }
}