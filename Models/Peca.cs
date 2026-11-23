using SQLite;
using System.Diagnostics.CodeAnalysis;

namespace CadastroClientes.Models;

public class Peca
{
    public int OS { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string CodigoPeca { get; set; } = string.Empty;
    public int Qtde { get; set; }
    public decimal Valor { get; set; }
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
}