using CadastroClientes.Data;
using CadastroClientes.Helpers;
using CadastroClientes.Models;
using CadastroClientes.Services;
using Microsoft.Maui.Networking;

namespace CadastroClientes.Views;

public partial class OrdemServicoPage : ContentPage
{
    private const double LarguraMinimaDesktop = 700;

    private readonly BancoLocal _bancoLocal;
    private readonly ApiService _apiService;
    //private readonly SincronizacaoService _sincronizacao;
    public record ItemLista(int Id, string Texto);
    private bool? _compacto;
    private List<Cliente> _todosClientes = new();

    public OrdemServicoPage(BancoLocal bancoLocal, ApiService apiService)
	{
		InitializeComponent();
        _bancoLocal = bancoLocal;
        _apiService = apiService;
        //_sincronizacao = sincronizacao;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await ListaClienteAsync(online: false);

        //bool online = await _sincronizacao.SincronizarAsync();

        //if (online)
        //    await ListaClienteAsync(online: true);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
    }

    private async void btnVoltarClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ClientesPage));
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (width <= 0)
            return;

        bool compacto = width < LarguraMinimaDesktop;

        if (compacto == _compacto)
            return;

        _compacto = compacto;
        //AplicarLayout(compacto);
    }

    private void OnFiltroTextoChanged(object? sender, TextChangedEventArgs e) => AplicarFiltro();

    private void OnFiltroSelecionado(object? sender, EventArgs e)
    {
        pFiltros.Title = pFiltros.SelectedIndex >= 0 ? string.Empty : "Filtrar por...";
        eFiltros.Text = null;
        AplicarFiltro();
    }

    private void OnExibirSelecionado(object? sender, EventArgs e)
    {
        pFiltrosAndamento.Title = pFiltrosAndamento.SelectedIndex >= 0 ? string.Empty : "Exibir apenas...";
        AplicarFiltro();
    }

    private void AplicarLayout(bool compacto)
    {
        if (compacto)
        {
            Raiz.ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star)
            };
            Raiz.RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            };

            Grid.SetRow(Conteudo, 1); Grid.SetColumn(Conteudo, 0);

            Conteudo.Padding = new Thickness(15);
            Conteudo.RowSpacing = 16;


            Logo.IsVisible = false;
        }
        else
        {
            Raiz.ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(new GridLength(240)),
                new ColumnDefinition(GridLength.Star)
            };
            Raiz.RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition(GridLength.Star)
            };

            Grid.SetRow(Conteudo, 0); Grid.SetColumn(Conteudo, 1);

            Conteudo.Padding = new Thickness(30);
            Conteudo.RowSpacing = 24;

            Logo.IsVisible = true;
        }
    }

    private async Task ListaClienteAsync(bool online)
    {
        try
        {
            _todosClientes = (await _bancoLocal.ObterClientesAsync())            
            .OrderByDescending(c => c.DtaEmissao)
            .ToList();

            AplicarFiltro();
            //TotalClientesLabel.Text = clientes.Count.ToString();

            //AtualizarResumoSincronizacao(online, clientes.Count(c => !c.Sincronizado));
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", $"Não foi possível carregar os clientes: {ex.Message}", "OK");
        }
    }

    private void AplicarFiltro()
    {
        string texto = (eFiltros.Text ?? string.Empty).Trim();
        string? campo = pFiltros.SelectedItem as string;
        string? exibicao = pFiltrosAndamento.SelectedItem as string;

        var cmp = StringComparison.CurrentCultureIgnoreCase;
        IEnumerable<Cliente> lista = _todosClientes;

        if (!string.IsNullOrEmpty(texto) && campo is not null)
        {
            lista = campo switch
            {
                "OS" => lista.Where(c => c.OS.ToString().Contains(texto)),
                "Nome" => lista.Where(c => (c.Nome ?? "").Contains(texto, cmp)),
                "Telefone" => lista.Where(c => SomenteDigitos(c.Telefone).Contains(SomenteDigitos(texto))),
                "Aparelho" => lista.Where(c => c.AparelhoTexto.Contains(texto, cmp)),
                _ => lista
            };
        }

        if (exibicao is null || exibicao == "Todos")
            lista = lista.Where(c => c.Andamento != "Entregue");
        else
            lista = lista.Where(c => string.Equals(c.Andamento, exibicao, cmp));

        cvClientes.ItemsSource = lista.ToList();
    }

    private static string SomenteDigitos(string? s) =>
        new string((s ?? "").Where(char.IsDigit).ToArray());



    private async void btnEditarClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is Cliente cliente)
        {
            await Navigation.PushAsync(new EditarOsPage(cliente));
        }
    }
}