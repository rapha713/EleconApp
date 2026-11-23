namespace CadastroClientes.Views;

using CadastroClientes.Data;
using CadastroClientes.Helpers;
using CadastroClientes.Models;
using System.Collections.ObjectModel;
using System.Globalization;
public partial class EditarOsPage : ContentPage
{
    private readonly Cliente _cliente;
    private readonly ObservableCollection<Peca> _pecas = new();
    private readonly BancoLocal _bancoLocal = new();
    private Peca? _pecaNovaEditar;
    private Peca? _pecaParaExcluir;
    private Dictionary<DatePicker, Entry> _paresData = null!;

    public EditarOsPage(Cliente cliente, Peca? peca = null, BancoLocal bancoLocal = null)
	{
		InitializeComponent();
        _cliente = cliente;
        _bancoLocal = bancoLocal ?? new BancoLocal();
        _paresData = new()
        {
            [pCompra] = eCompra,
            [pEmissao] = eEmissao
        };

        pAparelho.ItemsSource = ListasDados.Aparelhos.ToList();
        pAparelho.ItemDisplayBinding = new Binding(nameof(ItemLista.Nome));

        pMarca.ItemsSource = ListasDados.Marcas.ToList();
        pMarca.ItemDisplayBinding = new Binding(nameof(ItemLista.Nome));

        cvPecas.ItemsSource = _pecas;
        CarregarCampos();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await ListaDadosAsync(online: false);
        await CarregarPecasAsync();
        //bool online = await _sincronizacao.SincronizarAsync();

        //if (online)
        //    await ListaDadosAsync(online: true);
    }

    private async Task ListaDadosAsync(bool online)
    {
        try
        {
            var pecas = await _bancoLocal.ObterPecasCadastradasAsync();

            cvPecas.ItemsSource = pecas;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", $"Não foi possível carregar os dados: {ex.Message}", "OK");
        }
    }

    private void CarregarCampos()
    {
        lblTitulo.Text = $"Ordem de Serviço {_cliente.OS}";

        eOs.Text = _cliente.OS.ToString();
        cbExterno.IsChecked = _cliente.Externo;

        eNome.Text = _cliente.Nome;
        eTelefone.Text = _cliente.Telefone;
        eEmissao.Text = _cliente.DtaEmissao.ToString("dd/MM/yyyy");

        pAparelho.SelectedItem = ListasDados.Aparelhos.FirstOrDefault(a => a.Id == _cliente.Aparelho);
        pMarca.SelectedItem = ListasDados.Marcas.FirstOrDefault(a => a.Id == _cliente.Marca);
        eCodigo.Text = _cliente.Codigo;
        eSerie.Text = _cliente.NroSerie;
        cbGarantia.IsChecked = _cliente.Garantia;

        eDefeito.Text = _cliente.Defeito;
        eCondAparelho.Text = _cliente.CondAparelho;

        cbCabo.IsChecked = _cliente.CaboAc;
        cbControle.IsChecked = _cliente.CR;
        cbBase.IsChecked = _cliente.Base;
        cbSuporte.IsChecked = _cliente.Suporte;
        cbCaixaAc.IsChecked = _cliente.CaixaAcust;
        eOutroAcess.Text = _cliente.Outros;

        eRevenda.Text = _cliente.Revenda;
        eNroNF.Text = _cliente.NroNf;
        eCompra.Text = _cliente.DtaCompra.ToString("dd/MM/yyyy");

        string[] fluxo = { "Vist. Técnico", "Aprovado", "Peça Solicitada", "Pronto", "Entregue" };
        int atual = Array.IndexOf(fluxo, _cliente.Andamento);

        cbTecnico.IsChecked = atual >= 0;
        cbAprovado.IsChecked = atual >= 1;
        cbSolicitado.IsChecked = atual >= 2;
        cbPronto.IsChecked = atual >= 3;
        cbEntregue.IsChecked = atual >= 4;
        cbReprovado.IsChecked = _cliente.Andamento == "Reprovado";
    }

    private async void OnClickedSalvarOs(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(eNome.Text))
        {
            await DisplayAlertAsync("Erro", "Informe o nome do cliente.", "OK");
            eNome.Focus();
            return;
        }

        if (!DateTime.TryParseExact(eEmissao.Text, "dd/MM/yyyy",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var dataEmissao))
        {
            await DisplayAlertAsync("Erro", "Informe a data de emissão.", "OK");
            eEmissao.Focus();
            return;
        }

        bool garantia = cbGarantia.IsChecked;
        string outroAcessorio = eOutroAcess.Text?.Trim() ?? string.Empty;

        string andamento =
            cbEntregue.IsChecked ? "Entregue" :
            cbPronto.IsChecked ? "Pronto" :
            cbSolicitado.IsChecked ? "Peça Solicitada" :
            cbReprovado.IsChecked ? "Reprovado" :
            cbAprovado.IsChecked ? "Aprovado" :
            cbTecnico.IsChecked ? "Vist. Técnico" :
                                      "Cadastrado";

        _cliente.Nome = eNome.Text.Trim();
        _cliente.Telefone = eTelefone.Text?.Trim();
        _cliente.DtaEmissao = dataEmissao;

        if (pAparelho.SelectedItem is ItemLista ap)
            _cliente.Aparelho = ap.Id;

        if (pMarca.SelectedItem is ItemLista mc)
            _cliente.Aparelho = mc.Id;

        _cliente.Codigo = eCodigo.Text?.Trim();
        _cliente.NroSerie = eSerie.Text?.Trim();
        _cliente.Defeito = eDefeito.Text?.Trim();
        _cliente.CondAparelho = eCondAparelho.Text?.Trim();

        _cliente.Garantia = garantia;
        if (garantia)
        {
            _cliente.Revenda = eRevenda.Text?.Trim();
            _cliente.NroNf = eNroNF.Text?.Trim();
            _cliente.DtaCompra = DateTime.TryParseExact(eCompra.Text, "dd/MM/yyyy",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var compra)
                ? compra : DateTime.MinValue;
        }
        else
        {
            _cliente.Revenda = null;
            _cliente.NroNf = null;
            _cliente.DtaCompra = DateTime.MinValue;
        }

        _cliente.CaboAc = cbCabo.IsChecked;
        _cliente.CR = cbControle.IsChecked;
        _cliente.Base = cbBase.IsChecked;
        _cliente.Suporte = cbSuporte.IsChecked;
        _cliente.CaixaAcust = cbCaixaAc.IsChecked;
        _cliente.Outros = outroAcessorio;
        _cliente.Acessorio = _cliente.CaboAc || _cliente.CR || _cliente.Base
                          || _cliente.Suporte || _cliente.CaixaAcust
                          || !string.IsNullOrWhiteSpace(outroAcessorio);

        _cliente.Externo = cbExterno.IsChecked;
        _cliente.Andamento = andamento;

        try
        {
            await _bancoLocal.AtualizarClienteAsync(_cliente);
            await DisplayAlertAsync("Sucesso", $"OS {_cliente.OS} alterada!", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", $"Não foi possível salvar: {ex.Message}", "OK");
        }
    }

    private async void btnVoltarClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(OrdemServicoPage));
    }

    #region Add/Editar Peças
    private void OnAdicionarPecaClicked(object sender, EventArgs e) => AbrirGridModal();

    private void OnEditarPecaClicked(object sender, EventArgs e)
    {
        if (((Button)sender).BindingContext is Peca peca)
            AbrirGridModal(peca);
    }

    private void AbrirGridModal(Peca? peca = null)
    {
        _pecaNovaEditar = peca;

        lTituloGridModal.Text = peca is null ? "Adicionar peça" : "Editar peça";
        ePecaNome.Text = peca?.Nome ?? string.Empty;
        ePecaQtde.Text = peca?.Qtde.ToString() ?? "1";
        ePecaValor.Text = peca?.Valor.ToString("0.00", CultureInfo.GetCultureInfo("pt-BR")) ?? string.Empty;

        GridModal.IsVisible = true;
        ConfirmacaoExclusaoBorder.IsVisible = false;
        ModalBorderEdicao.IsVisible = true;
        ePecaNome.Focus();
    }

    private async void OnSalvarGridModal(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ePecaNome.Text))
        {
            await DisplayAlertAsync("Atenção", "Informe o nome da peça.", "OK");
            return;
        }
        if (!int.TryParse(ePecaQtde.Text, out int qtde) || qtde <= 0)
        {
            await DisplayAlertAsync("Atenção", "Quantidade inválida.", "OK");
            return;
        }

        decimal valor = LerMoeda(ePecaValor.Text);
        if (valor <= 0)
        {
            await DisplayAlertAsync("Atenção", "Valor inválido.", "OK");
            return;
        }

        var peca = _pecaNovaEditar ?? new Peca { OS = _cliente.OS };

        peca.Nome = ePecaNome.Text.Trim();
        peca.CodigoPeca = (ePecaCodigo.Text ?? string.Empty).Trim();
        peca.Qtde = qtde;
        peca.Valor = valor;

        await _bancoLocal.SalvarPecasAsync(peca);

        FecharGridModal();
        await CarregarPecasAsync();
    }

    private async Task CarregarPecasAsync()
    {
        _pecas.Clear();
        foreach (var p in await _bancoLocal.ObterPecasAsync(_cliente.OS))
            _pecas.Add(p);

        cvPecas.ItemsSource = _pecas;
    }
    #endregion

    #region Apagar Peças
    private void OnApagarPecaClicked(object sender, EventArgs e)
    {
        if (((Button)sender).BindingContext is not Peca peca) return;

        _pecaParaExcluir = peca;
        ModalBorderEdicao.IsVisible = false;
        ConfirmacaoExclusaoBorder.IsVisible = true;
        GridModal.IsVisible = true;
    }

    private async void OnExcluirPeca(object sender, EventArgs e)
    {
        if (_pecaParaExcluir is not null)
        {
            await _bancoLocal.RemoverPecasAsync(_pecaParaExcluir);
            _pecas.Remove(_pecaParaExcluir);
        }

        _pecaParaExcluir = null;
        FecharGridModal();
    }
    #endregion

    #region Nota Fiscal
    private void OnNotaFiscalClicked(object sender, EventArgs e)
    {
        GridNfModal.IsVisible = true;
    }

    private void OnCancelarNfModal(object sender, EventArgs e)
    {
        GridNfModal.IsVisible = false;
    }

    private void OnSalvarNfModal(object sender, EventArgs e)
    {
        _cliente.Revenda = eRevenda.Text;
        _cliente.NroNf = eNroNF.Text;
        if (DateTime.TryParseExact(eCompra.Text, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var data))
            _cliente.DtaCompra = data;
        GridNfModal.IsVisible = false;
    }
    #endregion

    #region Metodos Auxiliares
    private void FecharGridModal()
    {
        GridModal.IsVisible = false;
        _pecaNovaEditar = null;
    }

    private void OnCancelarGridModal(object sender, EventArgs e)
    {
        _pecaParaExcluir = null;
        FecharGridModal();
    }

    private static decimal LerMoeda(string? texto)
    {
        var digitos = new string((texto ?? "").Where(char.IsDigit).ToArray());
        return digitos.Length == 0 ? 0m : long.Parse(digitos) / 100m;
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
    #endregion
}