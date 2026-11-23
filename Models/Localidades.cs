using System.Diagnostics.CodeAnalysis;
using SQLite;

namespace CadastroClientes.Models;

public class Localidades
{
    [PrimaryKey]
    public int Id { get; set; }
    public string Cidade { get; set; } = string.Empty;
    public string UF { get; set; } = "SP";
    public string Pais { get; set; } = "Brasil";

    [Ignore] public string CidadeUF => $"{Cidade} - {UF}";
}