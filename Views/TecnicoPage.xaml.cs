using CadastroClientes.Data;
using CadastroClientes.Models;
using System.Collections.ObjectModel;
using System.Globalization;

namespace CadastroClientes.Views;

public partial class TecnicoPage : ContentPage
{
    private readonly Cliente _cliente;
    private readonly ObservableCollection<Peca> _pecas = new();
    private readonly BancoLocal _bancoLocal = new();
    private Peca? _pecaNovaEditar;
    private Peca? _pecaParaExcluir;

    private Cliente? _clienteNovaEditar;

    public TecnicoPage(Cliente cliente, Peca? peca = null, BancoLocal bancoLocal = null)
    {
        InitializeComponent();
        _cliente = cliente;
        _bancoLocal = bancoLocal ?? new BancoLocal();

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
        cbGarantia.IsChecked = _cliente.Garantia;
        eEmissao.Text = _cliente.DtaEmissao.ToString("dd/MM/yyyy");

        pAparelho.SelectedItem = ListasDados.Aparelhos.FirstOrDefault(a => a.Id == _cliente.Aparelho);
        pMarca.SelectedItem = ListasDados.Marcas.FirstOrDefault(a => a.Id == _cliente.Marca);
        eCodigo.Text = _cliente.Codigo;
        eSerie.Text = _cliente.NroSerie;

        eDefeito.Text = _cliente.Defeito;

        eCondAparelho.Text = _cliente.CondAparelho;

        cbCabo.IsChecked = _cliente.CaboAc;
        cbControle.IsChecked = _cliente.CR;
        cbBase.IsChecked = _cliente.Base;
        cbSuporte.IsChecked = _cliente.Suporte;
        cbCaixaAc.IsChecked = _cliente.CaixaAcust;
        eOutroAcess.Text = _cliente.Outros;
    }

    private async void OnClickedSalvarOs(object sender, EventArgs e)
    {
        bool garantia = cbGarantia.IsChecked;
        string outroAcessorio = eOutroAcess.Text?.Trim() ?? string.Empty;

        if (pAparelho.SelectedItem is ItemLista ap)
            _cliente.Aparelho = ap.Id;

        if (pMarca.SelectedItem is ItemLista mc)
            _cliente.Marca = mc.Id;

        _cliente.Codigo = eCodigo.Text?.Trim();
        _cliente.NroSerie = eSerie.Text?.Trim();
        _cliente.Defeito = eDefeito.Text?.Trim();
        _cliente.CondAparelho = eCondAparelho.Text?.Trim();
        _cliente.Garantia = garantia;

        _cliente.CaboAc = cbCabo.IsChecked;
        _cliente.CR = cbControle.IsChecked;
        _cliente.Base = cbBase.IsChecked;
        _cliente.Suporte = cbSuporte.IsChecked;
        _cliente.CaixaAcust = cbCaixaAc.IsChecked;
        _cliente.Outros = outroAcessorio;
        _cliente.Acessorio = _cliente.CaboAc || _cliente.CR || _cliente.Base
                          || _cliente.Suporte || _cliente.CaixaAcust
                          || !string.IsNullOrWhiteSpace(outroAcessorio);

        _cliente.Andamento = "Vist. Técnico";

        int totalPecas = (cvPecas.ItemsSource as IEnumerable<Peca>)?.Count() ?? 0;

        if (totalPecas == 0)
        {
            await DisplayAlertAsync("Alerta", "Nenhuma peça cadastrada! Cadastre alguma informação", "OK");
            return;
        }

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

    #region Modal Peças
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
        ePecaCodigo.Text = peca?.CodigoPeca ?? string.Empty;
        ePecaQtde.Text = peca?.Qtde.ToString() ?? "1";

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

        var peca = _pecaNovaEditar ?? new Peca { OS = _cliente.OS };

        peca.Nome = ePecaNome.Text.Trim();
        peca.CodigoPeca = (ePecaCodigo.Text ?? string.Empty).Trim();
        peca.Qtde = qtde;
        peca.Tecnico = 1;

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

    #region Modal Obs
    private void OnAdicionarObservacaoClicked(object sender, EventArgs e)
    {
        var cliente = (sender as BindableObject)?.BindingContext as Cliente ?? _cliente;

        AbrirGridModalObs(cliente);
    }

    private void AbrirGridModalObs(Cliente? cliente = null)
    {
        _clienteNovaEditar = cliente;

        bool temObservacao = !string.IsNullOrWhiteSpace(cliente?.Observacao);

        lTituloGridModalObs.Text = temObservacao ? "Editar observação" : "Adicionar observação";
        eObservacao.Text = cliente?.Observacao ?? string.Empty;

        GridModalObs.IsVisible = true;
        ModalBorderObs.IsVisible = true;
        eObservacao.Focus();
    }

    private async void OnSalvarGridModalObs(object sender, EventArgs e)
    {
        var cliente = _clienteNovaEditar ?? _cliente;
        if (cliente is null) return;

        string observacao = (eObservacao.Text ?? string.Empty).Trim();

        try
        {
            await _bancoLocal.AtualizarObservacaoAsync(cliente.OS, observacao);

            cliente.Observacao = observacao;
            FecharGridModalObs();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", $"Não foi possível salvar a observação: {ex.Message}", "OK");
        }
    }

    private void FecharGridModalObs()
    {
        GridModalObs.IsVisible = false;
        _pecaNovaEditar = null;
    }

    private void OnCancelarGridModalObs(object sender, EventArgs e)
    {
        _pecaParaExcluir = null;
        FecharGridModalObs();
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
    #endregion
}