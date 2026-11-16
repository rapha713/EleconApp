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
            var clientes = (await _bancoLocal.ObterClientesAsync()).OrderByDescending(c => c.DtaEmissao).ToList();

            cvClientes.ItemsSource = clientes;
            //TotalClientesLabel.Text = clientes.Count.ToString();

            //AtualizarResumoSincronizacao(online, clientes.Count(c => !c.Sincronizado));
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", $"Não foi possível carregar os clientes: {ex.Message}", "OK");
        }
    }

    private async void btnEditarClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is Cliente cliente)
        {
            await Navigation.PushAsync(new EditarOsPage(cliente));
        }
    }
}