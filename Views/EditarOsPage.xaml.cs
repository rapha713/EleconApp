namespace CadastroClientes.Views;
using CadastroClientes.Models;
using System.Collections.ObjectModel;
using System.Globalization;
public partial class EditarOsPage : ContentPage
{
    private readonly Cliente _cliente;
    private readonly ObservableCollection<Peca> _pecas = new();
    private Peca? _pecaNovaEditar;
    private Peca? _pecaParaExcluir;
    private Dictionary<DatePicker, Entry> _paresData = null!;

    public EditarOsPage(Cliente cliente, Peca? peca = null)
	{
		InitializeComponent();
        _cliente = cliente;
        _paresData = new()
        {
            [pCompra] = eCompra,
            [pEmissao] = eEmissao
        };

        _pecas.Add(new Peca { Nome = "Fonte 12V", Qtde = 1, Valor = 45.90m });
        _pecas.Add(new Peca { Nome = "Capacitor 470uF", Qtde = 4, Valor = 3.50m });
        _pecas.Add(new Peca { Nome = "Placa principal", Qtde = 1, Valor = 189.00m });
        _pecas.Add(new Peca { Nome = "Placa principal", Qtde = 1, Valor = 19.00m });
        _pecas.Add(new Peca { Nome = "Placa principal", Qtde = 1, Valor = 390.00m });

        cvPecas.ItemsSource = _pecas;

        pAparelho.ItemsSource = ListasFixas.Aparelhos.ToList();
        pAparelho.ItemDisplayBinding = new Binding(nameof(ItemLista.Nome));

        cvPecas.ItemsSource = _pecas;

        CarregarCampos();
    }

    private void CarregarCampos()
    {
        lblTitulo.Text = $"Ordem de Serviço {_cliente.OS}";

        eOs.Text = _cliente.OS.ToString();
        cbExterno.IsChecked = _cliente.Externo;

        eNome.Text = _cliente.Nome;
        eTelefone.Text = _cliente.Telefone;
        eEmissao.Text = _cliente.DtaEmissao.ToString("dd/MM/yyyy");

        pAparelho.SelectedItem = ListasFixas.Aparelhos.FirstOrDefault(a => a.Id == _cliente.Aparelho);
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
    }

    private async void btnVoltarClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(OrdemServicoPage));
    }

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

    private void OnApagarPecaClicked(object sender, EventArgs e)
    {
        if (((Button)sender).BindingContext is not Peca peca) return;

        _pecaParaExcluir = peca;
        AbrirGridModalExclusao();
    }

    private void AbrirGridModalExclusao()
    {
        ModalBorderEdicao.IsVisible = false;
        ConfirmacaoExclusaoBorder.IsVisible = true;
        GridModal.IsVisible = true;
    }

    private void OnExcluirPeca(object sender, EventArgs e)
    {
        if (_pecaParaExcluir is not null)
            _pecas.Remove(_pecaParaExcluir);

        _pecaParaExcluir = null;
        FecharGridModal();
    }

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

    private async void OnSalvarGridModal(object sender, EventArgs e)
    {
        var pt = CultureInfo.GetCultureInfo("pt-BR");

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
        if (!decimal.TryParse(ePecaValor.Text, NumberStyles.Any, pt, out decimal valor))
        {
            await DisplayAlertAsync("Atenção", "Valor inválido.", "OK");
            return;
        }

        var nova = new Peca { Nome = ePecaNome.Text.Trim(), Qtde = qtde, Valor = valor };

        if (_pecaNovaEditar is null)
            _pecas.Add(nova);
        else
        {
            int i = _pecas.IndexOf(_pecaNovaEditar);
            if (i >= 0) _pecas[i] = nova;
        }

        FecharGridModal();
    }

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
}