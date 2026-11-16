using CadastroClientes.Data;
using CadastroClientes.Helpers;
using CadastroClientes.Models;
using CadastroClientes.Services;
using Microsoft.Maui.Networking;

namespace CadastroClientes.Views;

public partial class ClientesPage : ContentPage
{
    private const double LarguraMinimaDesktop = 700;

    private readonly BancoLocal _bancoLocal;
    private readonly ApiService _apiService;
    //private readonly SincronizacaoService _sincronizacao;

    public record ItemLista(int Id, string Texto);
    private bool? _compacto;

    public ClientesPage(BancoLocal bancoLocal, ApiService apiService) //, SincronizacaoService sincronizacao
    {
        InitializeComponent();

        _bancoLocal = bancoLocal;
        _apiService = apiService;
        //_sincronizacao = sincronizacao;
    }

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
            _cidades.FirstOrDefault(c => c.Id == id)?.Texto ?? string.Empty;

        public static string ObterAparelho(int id) =>
            _aparelhos.FirstOrDefault(a => a.Id == id)?.Texto ?? string.Empty;
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


    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (width <= 0)
            return;

        bool compacto = width < LarguraMinimaDesktop;

        if (compacto == _compacto)
            return;

        _compacto = compacto;
        AplicarLayout(compacto);
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

            Grid.SetRow(Menu, 0); Grid.SetColumn(Menu, 0);
            Grid.SetRow(Conteudo, 1); Grid.SetColumn(Conteudo, 0);

            Menu.Padding = new Thickness(10);
            Conteudo.Padding = new Thickness(15);
            Conteudo.RowSpacing = 16;

            MenuItens.Orientation = StackOrientation.Horizontal;
            MenuScroll.Orientation = ScrollOrientation.Horizontal;

            Logo.IsVisible = false;
            TituloMenu.IsVisible = false;
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

            Grid.SetRow(Menu, 0); Grid.SetColumn(Menu, 0);
            Grid.SetRow(Conteudo, 0); Grid.SetColumn(Conteudo, 1);

            Menu.Padding = new Thickness(20);
            Conteudo.Padding = new Thickness(30);
            Conteudo.RowSpacing = 24;

            MenuItens.Orientation = StackOrientation.Vertical;
            MenuScroll.Orientation = ScrollOrientation.Vertical;

            Logo.IsVisible = true;
            TituloMenu.IsVisible = true;
        }
    }

    private async void OnNovoClienteClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(CadastroClientePage));
    }

    private async void OnOrdemServicoClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(OrdemServicoPage));
    }

    private async void OnSairClicked(object sender, EventArgs e)
    {
        
    }

    //private async void OnSincronizarClienteClicked(object sender, EventArgs e)
    //{
    //    bool online = await _sincronizacao.SincronizarAsync();

    //    if (online)
    //        await ListaClienteAsync(online: true);
    //}

    private async Task ListaClienteAsync(bool online)
    {
        try
        {
            var clientes = (await _bancoLocal.ObterClientesAsync())
                .OrderByDescending(c => c.DtaEmissao).ToList();

            cvClientes.ItemsSource = clientes;
           //TotalClientesLabel.Text = clientes.Count.ToString();

            //AtualizarResumoSincronizacao(online, clientes.Count(c => !c.Sincronizado));
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", $"Não foi possível carregar os clientes: {ex.Message}", "OK");
        }
    }
}