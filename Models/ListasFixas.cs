namespace CadastroClientes.Models;

public static class ListasFixas
{
    private static readonly List<ItemLista> _cidades = new()
    {
        new(1, "Itu"),
        new(2, "Salto"),
        new(3, "Indaiatuba")
    };

    private static readonly List<ItemLista> _aparelhos = new()
    {
        new(1, "Televisão"),
        new(2, "Radio"),
        new(3, "Micro-ondas"),
        new(4, "Outros")
    };

    public static string ObterCidade(int id) =>
        _cidades.FirstOrDefault(c => c.Id == id)?.Nome ?? string.Empty;

    public static string ObterAparelho(int id) =>
        _aparelhos.FirstOrDefault(a => a.Id == id)?.Nome ?? string.Empty;

    public static IReadOnlyList<ItemLista> Cidades => _cidades;
    public static IReadOnlyList<ItemLista> Aparelhos => _aparelhos;
}