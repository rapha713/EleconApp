using System.Diagnostics.CodeAnalysis;
using SQLite;

namespace CadastroClientes.Models;

public class Aparelhos
{
    [PrimaryKey]
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int Status { get; set; }

    [Ignore] public bool Ativo => Status == 1;
    [Ignore] public bool Inativo => Status != 1;
    [Ignore] public string StatusTexto => Ativo ? "Ativo" : "Inativo";
}