using SQLite;

namespace CadastroClientes.Models;

[Table("Clientes")]
public class Cliente
{
    [NotNull]
    public Guid Id { get; set; }
    [PrimaryKey, AutoIncrement]
    public int OS { get; set; }
    [NotNull]
    public string Nome { get; set; }
    public string? Telefone { get; set; }
    public string? Endereco { get; set; }
    public string? Bairro { get; set; }
    public int Cidade { get; set; }
    public int? CEP { get; set; }
    [NotNull]
    public DateTime DtaEmissao { get; set; } = DateTime.Now;
    [NotNull]
    public int Aparelho { get; set; }
    public int Marca { get; set; }
    public string? Codigo { get; set; }
    public string? NroSerie { get; set; }
    public string? Defeito { get; set; }
    public string? CondAparelho { get; set; }
    public bool Garantia { get; set; }
    public string? Revenda { get; set; }
    public string? NroNf { get; set; }
    public DateTime DtaCompra { get; set; }
    public bool Acessorio { get; set; }
    public bool CaboAc { get; set; }
    public bool CR { get; set; }
    public bool Base { get; set; }
    public bool Suporte { get; set; }
    public bool CaixaAcust { get; set; }
    public string? Outros { get; set; }
    public bool Externo { get; set; }
    public string? Andamento { get; set; }

    [Ignore]
    public string AcessoriosTexto
    {
        get
        {
            var itens = new List<string>();

            if (CaboAc) itens.Add("Cabo AC");
            if (CR) itens.Add("CR");
            if (Base) itens.Add("Base");
            if (Suporte) itens.Add("Suporte");
            if (CaixaAcust) itens.Add("Caixa Acústica");
            if (!string.IsNullOrWhiteSpace(Outros)) itens.Add(Outros.Trim());

            return itens.Count > 0 ? string.Join(", ", itens) : "S/Acessório";
        }
    }

    [Ignore]
    public string MarcaTexto => ListasDados.ObterMarca(Marca);

    [Ignore]
    public string AparelhoTexto
    {
        get
        {
            var aparelho = ListasDados.ObterAparelho(Aparelho);
            return string.IsNullOrWhiteSpace(Codigo) ? aparelho : $"{aparelho}  - {Codigo.Trim()}";
        }
    }

    [Ignore]
    public string MarcaAparelhoTexto
    {
        get
        {
            var aparelho = ListasDados.ObterAparelho(Aparelho);
            var marca = ListasDados.ObterMarca(Marca);

            return string.IsNullOrWhiteSpace(Codigo) ? aparelho : $"{aparelho} {marca} - {Codigo.Trim()}";
        }
    }

    [Ignore]
    public string CidadeTexto => ListasDados.ObterCidade(Cidade);
}