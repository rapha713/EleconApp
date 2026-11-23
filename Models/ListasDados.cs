namespace CadastroClientes.Models;

public static class ListasDados
{
    private static Dictionary<int, string> _cidades = new();
    private static Dictionary<int, string> _aparelhos = new();
    private static Dictionary<int, string> _marca = new();
    public static IReadOnlyList<ItemLista> Cidades { get; private set; } = new List<ItemLista>();
    public static IReadOnlyList<ItemLista> Aparelhos { get; private set; } = new List<ItemLista>();
    public static IReadOnlyList<ItemLista> Marcas { get; private set; } = new List<ItemLista>();

    public static void Carregar(IEnumerable<Localidades> cidades, IEnumerable<Aparelhos> aparelhos, IEnumerable<Marcas> marcas)
    {
        _cidades = cidades.ToDictionary(c => c.Id, c => c.Cidade);
        _aparelhos = aparelhos.ToDictionary(a => a.Id, a => a.Nome);
        _marca = marcas.ToDictionary(a => a.Id, a => a.Nome);

        Cidades = cidades.OrderBy(c => c.Cidade).Select(c => new ItemLista(c.Id, c.Cidade)).ToList();
        Aparelhos = aparelhos.Where(a => a.Status == 1).OrderBy(a => a.Nome).Select(a => new ItemLista(a.Id, a.Nome)).ToList();
        Marcas = marcas.Where(a => a.Status == 1).OrderBy(a => a.Nome).Select(a => new ItemLista(a.Id, a.Nome)).ToList();
    }

    public static string ObterCidade(int id) => _cidades.TryGetValue(id, out var nome) ? nome : string.Empty;

    public static string ObterAparelho(int id) => _aparelhos.TryGetValue(id, out var nome) ? nome : string.Empty;

    public static string ObterMarca(int id) => _marca.TryGetValue(id, out var nome) ? nome : string.Empty;
}