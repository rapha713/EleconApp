using System;
using System.Collections.Generic;
using System.Text;

namespace CadastroClientes.Models;

public class ClienteApi
{
    public Guid? IdSincronizacao { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string TipoPessoa { get; set; } = string.Empty;
    public string? Documento { get; set; }
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public DateTime? DataNascimento { get; set; }
    public string? Endereco { get; set; }
    public string? Bairro { get; set; }
    public string? UF { get; set; }
    public string? Cidade { get; set; }
    public string? Profissao { get; set; }
    public string? EstadoCivil { get; set; }
    public DateTime? DataCriacao { get; set; }
    public bool Excluido { get; set; }
    public DateTime? DataAtualizacao { get; set; }
}