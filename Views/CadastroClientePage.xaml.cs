using CadastroClientes.Data;
using CadastroClientes.Helpers;
using CadastroClientes.Models;
using CadastroClientes.Services;
using System.Globalization;
using System.Net.Http;

namespace CadastroClientes.Views;

public partial class CadastroClientePage : ContentPage
{
    private readonly BancoLocal _bancoLocal;
    private readonly ApiService _apiService;
    private Dictionary<DatePicker, Entry> _paresData = null!;

    public CadastroClientePage(ApiService apiService)
	{
		InitializeComponent();
        _bancoLocal = new BancoLocal();
        _apiService = apiService;
        _paresData = new()
        {
            [pCompra] = eCompra,
            [pEmissao] = eEmissao
        };
        pCompra.MaximumDate = DateTime.Today;
        pEmissao.Date = DateTime.Today;
        eEmissao.Text = DateTime.Today.ToString("dd/MM/yyyy");

        pCidade.ItemsSource = _cidades;
        pCidade.ItemDisplayBinding = new Binding(nameof(ItemLista.Nome));

        pAparelho.ItemsSource = _aparelhos;
        pAparelho.ItemDisplayBinding = new Binding(nameof(ItemLista.Nome));
    }

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

    private async void OnCadastrarClicked(object sender, EventArgs e)
    {
        bool externo = cbExterno.IsChecked;
        string nome = eNome.Text?.Trim() ?? string.Empty;
        string telefone = Mascara.SomenteDigitos(eTelefone.Text);
        string cep = Mascara.SomenteDigitos(eCep.Text);
        DateTime emissao = eEmissao.Text != null && DateTime.TryParse(eEmissao.Text, out var parsedEmissao) ? parsedEmissao : DateTime.Now;
        DateTime compra = eCompra.Text != null && DateTime.TryParse(eCompra.Text, out var parsedCompra) ? parsedCompra : DateTime.MinValue;

        bool garantia = cbGarantia.IsChecked;
        bool caboAc = cbCabo.IsChecked;
        bool controle = cbControle.IsChecked;
        bool baseT = cbBase.IsChecked;
        bool suporte = cbSuporte.IsChecked;
        bool caixaAc = cbCaixaAc.IsChecked;
        string outroAcessorio = eOutroAcess.Text?.Trim() ?? string.Empty;

        bool acessorio = false;

        if (string.IsNullOrWhiteSpace(nome))
        {
            await DisplayAlertAsync("Erro", "Informe o nome do cliente.", "OK");
            eNome.Focus();
            return;
        }

        if (pCidade.SelectedItem is not ItemLista cidade)
        {
            await DisplayAlertAsync("Erro", "Informe a cidade.", "OK");
            pCidade.Focus();
            return;
        }

        if (pAparelho.SelectedItem is not ItemLista aparelho)
        {
            await DisplayAlertAsync("Erro", "Informe o aparelho.", "OK");
            pAparelho.Focus();
            return;
        }

        if (emissao == DateTime.MinValue)
        {
            await DisplayAlertAsync("Erro", "Informe a data de emissão.", "OK");
            eEmissao.Focus();
            return;
        }

        if (caboAc || controle || baseT || suporte || caixaAc || !string.IsNullOrWhiteSpace(outroAcessorio))
            acessorio = true;

        int idCidade = cidade.Id;
        int idAparelho = aparelho.Id;

        try
        {
            Guid identificador = Guid.NewGuid();

            var cliente = new Cliente
            {
                Id = identificador,
                OS = 1,
                Nome = nome,
                Telefone = telefone,
                Endereco = eEndereco.Text?.Trim(),
                Bairro = eBairro.Text?.Trim(),
                Cidade = idCidade,
                CEP = int.Parse(cep ?? "null"),
                DtaEmissao = emissao,
                Aparelho = idAparelho,
                Codigo = eCodigo.Text?.Trim(),
                NroSerie = eSerie.Text?.Trim(),
                Defeito = eDefeito.Text?.Trim(),
                CondAparelho = eCondAparelho.Text?.Trim(),
                Garantia = garantia,
                Revenda = eRevenda.Text?.Trim(),
                NroNf = eNroNf.Text?.Trim(),
                DtaCompra = compra,
                Acessorio = acessorio,
                CaboAc = caboAc,
                CR = controle,
                Base = baseT,
                Suporte = suporte,
                CaixaAcust = caixaAc,
                Outros = outroAcessorio,
                Externo = externo,
                Andamento = "Cadastrado"
            };

            //bool conectado;

            //try
            //{
            //    conectado = await _apiService.ObterConexaoAsync();
            //}
            //catch (HttpRequestException)
            //{
            //    conectado = false;
            //}

            //if (conectado)
            //{
            //    bool salvo = await _apiService.CadastrarClienteAsync(cliente);

            //    if (!salvo)
            //    {
            //        await DisplayAlertAsync("Erro", "Não foi possível cadastrar o cliente no servidor.", "OK");

            //        return;
            //    }

            //    await DisplayAlertAsync("Sucesso", "Cliente cadastrado no servidor!", "OK");
            //}
            //else
            //{
                await _bancoLocal.SalvarClienteAsync(cliente);

                await DisplayAlertAsync("Sucesso", "Cliente salvo localmente!", "OK");
            //}

            LimparCampos();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", $"Não foi possível cadastrar o cliente: {ex.Message}", "OK");
        }

        await Shell.Current.GoToAsync(nameof(ClientesPage));
    }

    private async void OnVoltarClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ClientesPage));
    }

    private void LimparCampos()
    {
        cbExterno.IsChecked = false;
        eNome.Text = string.Empty;
        eTelefone.Text = string.Empty;
        eCep.Text = string.Empty;
        pAparelho.SelectedItem = null;
        eTelefone.Text = string.Empty;
        eEndereco.Text = string.Empty;
        eBairro.Text = string.Empty;
        pAparelho.SelectedItem = null;
        eCodigo.Text = string.Empty;
        eSerie.Text = string.Empty;
        eDefeito.Text = string.Empty;
        eCondAparelho.Text = string.Empty;
        eRevenda.Text = string.Empty;
        eNroNf.Text = string.Empty;
        cbCabo.IsChecked = false;
        cbControle.IsChecked = false;
        cbBase.IsChecked = false;
        cbSuporte.IsChecked = false;
        cbCaixaAc.IsChecked = false;
        eOutroAcess.Text = string.Empty;
    }

    private const double LarguraMinimaDesktop = 700;
    private bool? _compacto;

    private readonly record struct Posicao(int Linha, int Coluna, int Span);

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
            GridCampos.RowDefinitions = CriarLinhas(15);
            GridCampos.RowSpacing = 18;
            Formulario.Padding = new Thickness(15);

            lblTitulo.FontSize = 24;
            lblTitulo.Margin = new Thickness(0, 15, 0, 0);

            Posicionar(cOs, new(0, 0, 6)); //Linha, Coluna e Tamanho (Span)
            Posicionar(cExterno, new(0, 6, 6));
            Posicionar(cNome, new(1, 0, 12));
            Posicionar(cTelefone, new(2, 0, 6));
            Posicionar(cEmissao, new(2, 6, 6));
            Posicionar(cEndereco, new(3, 0, 12));
            Posicionar(cBairro, new(4, 0, 12));
            Posicionar(cCidade, new(5, 0, 8));
            Posicionar(cCep, new(5, 8, 4));
            Posicionar(cAparelho, new(6, 0, 6));
            Posicionar(cCodigo, new(6, 6, 6));
            Posicionar(cSerie, new(7, 0, 12));
            Posicionar(cDefeito, new(8, 0, 12));
            Posicionar(cCondAparelho, new(9, 0, 12));
            Posicionar(cGarantia, new(10, 0, 4));
            Posicionar(cRevenda, new(10, 4, 8));
            Posicionar(cNroNf, new(11, 0, 6));
            Posicionar(cCompra, new(11, 6, 6));
            Posicionar(cCabo, new(12, 0, 3));
            Posicionar(cControle, new(12, 3, 3));
            Posicionar(cBase, new(12, 6, 3));
            Posicionar(cSuporte, new(12, 9, 3));
            Posicionar(cCaixaAc, new(13, 0, 4));
            Posicionar(cOutroAcess, new(13, 4, 8));
            Posicionar(cBotoes, new(14, 0, 12));

            bEmissao.WidthRequest = 150;
            bEmissao.HorizontalOptions = LayoutOptions.Start;
            eEmissao.WidthRequest = 145;

            bCompra.WidthRequest = 150;
            bCompra.HorizontalOptions = LayoutOptions.Start;
            eCompra.WidthRequest = 145;
        }
        else
        {
            GridCampos.RowDefinitions = CriarLinhas(9);
            GridCampos.RowSpacing = 35;
            Formulario.Padding = new Thickness(50);

            lblTitulo.FontSize = 30;
            lblTitulo.Margin = new Thickness(0, 30, 0, 0);

            Posicionar(cOs, new(0, 0, 2));
            Posicionar(cExterno, new(0, 2, 2));
            Posicionar(cNome, new(1, 0, 6));
            Posicionar(cTelefone, new(1, 6, 3));
            Posicionar(cEmissao, new(1, 9, 3));
            Posicionar(cEndereco, new(2, 0, 8));
            Posicionar(cBairro, new(2, 8, 4));
            Posicionar(cCidade, new(3, 0, 3));
            Posicionar(cCep, new(3, 3, 2));
            Posicionar(cAparelho, new(3, 5, 2));
            Posicionar(cCodigo, new(3, 7, 2));
            Posicionar(cSerie, new(3, 9, 3));
            Posicionar(cDefeito, new(4, 0, 12));
            Posicionar(cCondAparelho, new(5, 0, 12));
            Posicionar(cGarantia, new(6, 0, 2));
            Posicionar(cRevenda, new(6, 2, 3));
            Posicionar(cNroNf, new(6, 5, 4));
            Posicionar(cCompra, new(6, 9, 3));
            Posicionar(cCabo, new(7, 0, 1));
            Posicionar(cControle, new(7, 1, 1));
            Posicionar(cBase, new(7, 2, 1));
            Posicionar(cSuporte, new(7, 3, 1));
            Posicionar(cCaixaAc, new(7, 4, 2));
            Posicionar(cOutroAcess, new(7, 5, 5));
            Posicionar(cBotoes, new(8, 0, 12));

            bEmissao.WidthRequest = 150;
            bEmissao.HorizontalOptions = LayoutOptions.Start;
            eEmissao.WidthRequest = 145;

            bCompra.WidthRequest = 150;
            bCompra.HorizontalOptions = LayoutOptions.Start;
            eCompra.WidthRequest = 145;
        }
    }

    private void onGarantia_Changed(object sender, CheckedChangedEventArgs e)
    {
        bool garantia = e.Value;
        var fundo = garantia ? Colors.White : Colors.LightGray;

        eRevenda.IsEnabled = garantia;
        eRevenda.BackgroundColor = fundo;

        eNroNf.IsEnabled = garantia;
        eNroNf.BackgroundColor = fundo;

        eCompra.IsEnabled = garantia;
        pCompra.IsEnabled = garantia;
        bCompra.BackgroundColor = fundo;
        lblIconeCompra.Opacity = garantia ? 1 : 0.4;

        if (!garantia)
        {
            eRevenda.Text = string.Empty;
            eNroNf.Text = string.Empty;
            eCompra.Text = string.Empty;
            pCompra.Date = null;
        }
    }

    private void OnDataSelecionada(object? sender, DateChangedEventArgs e)
    {
        if (sender is not DatePicker picker)
            return;

        DateTime? nova = e.NewDate;

        if (nova is null)
            return;

        if (_paresData.TryGetValue(picker, out var entry))
            entry.Text = nova.Value.ToString("dd/MM/yyyy");
    }

    private void OnDataTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is not Entry entry)
            return;

        var picker = _paresData.FirstOrDefault(p => p.Value == entry).Key;

        if (picker is null)
            return;

        if (DateTime.TryParseExact(e.NewTextValue, "dd/MM/yyyy",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var data)
            && data >= picker.MinimumDate
            && data <= picker.MaximumDate)
        {
            picker.Date = data;
        }
    }

    private static void Posicionar(View view, Posicao p)
    {
        Grid.SetRow(view, p.Linha);
        Grid.SetColumn(view, p.Coluna);
        Grid.SetColumnSpan(view, p.Span);
    }

    private static RowDefinitionCollection CriarLinhas(int quantidade)
    {
        var linhas = new RowDefinitionCollection();
        for (int i = 0; i < quantidade; i++)
            linhas.Add(new RowDefinition(GridLength.Auto));
        return linhas;
    }
}