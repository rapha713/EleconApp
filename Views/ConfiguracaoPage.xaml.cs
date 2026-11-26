using CadastroClientes.Data;
using CadastroClientes.Models;
using System.Collections.ObjectModel;
using System.Globalization;

namespace CadastroClientes.Views;

public partial class ConfiguracaoPage : ContentPage
{
    private readonly BancoLocal _bancoLocal;

    private readonly ObservableCollection<Localidades> _localidades = new();
    private Localidades? _localidadesNovaEditar;
    private Localidades? _localidadesParaExcluir;

    private Aparelhos? _aparelhosNovaEditar;
    private Aparelhos? _aparelhosParaDesativar;

    private Marcas? _marcasNovaEditar;
    private Marcas? _marcasParaDesativar;

    public ConfiguracaoPage(BancoLocal bancoLocal)
	{
		_bancoLocal = bancoLocal;
		InitializeComponent();
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await ListaDadosAsync(online: false);

        //bool online = await _sincronizacao.SincronizarAsync();

        //if (online)
        //    await ListaClienteAsync(online: true);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
    }

    private async Task ListaDadosAsync(bool online)
    {
        try
        {
            var cidades = (await _bancoLocal.ObterLocalidadesAsync())
                .OrderByDescending(c => c.Id).ToList();

            cvLocalidades.ItemsSource = cidades;
            TotalCidadesSpan.Text = cidades.Count.ToString();
            //AtualizarResumoSincronizacao(online, cidades.Count(c => !c.Sincronizado));

            var aparelhos = (await _bancoLocal.ObterAparelhosAsync())
                .OrderByDescending(a => a.Id).ToList();

            cvAparelhos.ItemsSource = aparelhos;
            TotalAparelhosSpan.Text = aparelhos.Count.ToString();
            //AtualizarResumoSincronizacao(online, aparelhos.Count(a => !a.Sincronizado));

            var marcas = (await _bancoLocal.ObterMarcasAsync()).OrderBy(a => a.Nome).ToList();

            cvMarcas.ItemsSource = marcas;
            TotalMarcasSpan.Text = marcas.Count.ToString();
            //AtualizarResumoSincronizacao(online, aparelhos.Count(a => !a.Sincronizado));
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", $"Não foi possível carregar as cidades: {ex.Message}", "OK");
        }
    }

    private async void OnPaginaInicialClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ClientesPage));
    }

    private async void OnOrdemServicoClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(OrdemServicoPage));
    }

    private async void OnFuncionarioClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(FuncionarioPage));
    }

    private async void OnSairClicked(object sender, EventArgs e)
    {
        bool ok = await DisplayAlertAsync("Sair", "Deseja realmente fechar o programa?", "Sim", "Não");
        if (!ok) return;

        Application.Current?.Quit();
    }

    #region Localidades
    private void OnAdicionarCidadeClicked(object sender, EventArgs e) => AbrirCidadeModal();
    private void OnEditarCidadeClicked(object sender, EventArgs e)
    {
        if (((Button)sender).BindingContext is Localidades localidade)
            AbrirCidadeModal(localidade);
    }
    private void AbrirCidadeModal(Localidades? localidades = null)
    {
        _localidadesNovaEditar = localidades;

        lTituloModalCidade.Text = localidades is null ? "Adicionar cidade" : "Editar cidade";
        eCidade.Text = localidades?.Cidade ?? string.Empty;
        eUF.Text = localidades?.UF ?? "SP";

        GridModal.IsVisible = true;
        ModalCidade.IsVisible = true;
        ModalAparelho.IsVisible = false;
        ModalMarca.IsVisible = false;
        ModalExcluirCidade.IsVisible = false;
        eCidade.Focus();
    }

    private async void OnSalvarGridCidade(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(eCidade.Text))
        {
            await DisplayAlertAsync("Atenção", "Informe o nome da cidade.", "OK");
            return;
        }

        string cidade = eCidade.Text.Trim();
        string uf = (eUF.Text ?? string.Empty).Trim().ToUpper();

        if (_localidadesNovaEditar is null)
        {
            await _bancoLocal.SalvarLocalidadeAsync(new Localidades { Cidade = cidade, UF = uf });
        }
        else
        {
            _localidadesNovaEditar.Cidade = cidade;
            _localidadesNovaEditar.UF = uf;
            await _bancoLocal.SalvarLocalidadeAsync(_localidadesNovaEditar);
        }

        FecharGridModal();
        await ListaDadosAsync(false);
    }
    private void OnExcluirCidadeClicked(object sender, EventArgs e)
    {
        if (((Button)sender).BindingContext is not Localidades localidade) return;

        _localidadesParaExcluir = localidade;
        lConfirmaExclusaoCidade.Text = $"Deseja realmente excluir \"{localidade.Cidade}\"?";

        ModalCidade.IsVisible = false;
        ModalAparelho.IsVisible = false;
        ModalExcluirCidade.IsVisible = true;
        GridModal.IsVisible = true;
    }

    private async void OnConfirmarExcluirCidade(object sender, EventArgs e)
    {
        if (_localidadesParaExcluir is not null)
            await _bancoLocal.RemoverLocalidadeAsync(_localidadesParaExcluir);

        FecharGridModal();
        await ListaDadosAsync(false);
    }


    #endregion

    #region Aparelhos
    private void OnAdicionarAparelhoClicked(object sender, EventArgs e) => AbrirAparelhoModal();

    private void OnEditarAparelhoClicked(object sender, EventArgs e)
    {
        if (((Button)sender).BindingContext is Aparelhos aparelho)
            AbrirAparelhoModal(aparelho);
    }
    private void AbrirAparelhoModal(Aparelhos? aparelhos = null)
    {
        _aparelhosNovaEditar = aparelhos;

        lTituloModalAparelho.Text = aparelhos is null ? "Adicionar aparelho" : "Editar aparelho";
        eAparelho.Text = aparelhos?.Nome ?? string.Empty;
        pStatus.SelectedIndex = aparelhos is null || aparelhos.Status == 1 ? 0 : 1;

        GridModal.IsVisible = true;
        ModalAparelho.IsVisible = true;
        ModalMarca.IsVisible = false;
        ModalCidade.IsVisible = false;
        eAparelho.Focus();
    }
    private async void OnSalvarGridAparelho(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(eAparelho.Text))
        {
            await DisplayAlertAsync("Atenção", "Informe o aparelho.", "OK");
            return;
        }

        int status = pStatus.SelectedIndex == 1 ? 0 : 1;

        if (_aparelhosNovaEditar is null)
        {
            await _bancoLocal.SalvarAparelhoAsync(new Aparelhos { Nome = eAparelho.Text.Trim(), Status = status });
        }
        else
        {
            _aparelhosNovaEditar.Nome = eAparelho.Text.Trim();
            _aparelhosNovaEditar.Status = status;
            await _bancoLocal.SalvarAparelhoAsync(_aparelhosNovaEditar);
        }

        FecharGridModal();
        await ListaDadosAsync(false);
    }
    private async void OnAlterarStatusAparelhoClicked(object sender, EventArgs e)
    {
        if (((Button)sender).BindingContext is not Aparelhos aparelho) return;

        bool ativar = aparelho.Status != 1;
        string acao = ativar ? "ativar" : "desativar";

        bool ok = await DisplayAlertAsync("Confirmar", $"Deseja {acao} \"{aparelho.Nome}\"?", "Sim", "Não");
        if (!ok) return;

        aparelho.Status = ativar ? 1 : 0;
        await _bancoLocal.SalvarAparelhoAsync(aparelho);
        await ListaDadosAsync(false);
    }

    #endregion

    #region Marcas
    private void OnAdicionarMarcaClicked(object sender, EventArgs e) => AbrirMarcaModal();

    private void OnEditarMarcaClicked(object sender, EventArgs e)
    {
        if (((Button)sender).BindingContext is Marcas marca)
            AbrirMarcaModal(marca);
    }
    private void AbrirMarcaModal(Marcas? marcas = null)
    {
        _marcasNovaEditar = marcas;

        lTituloModalMarca.Text = marcas is null ? "Adicionar marca" : "Editar marca";
        eMarca.Text = marcas?.Nome ?? string.Empty;
        pStatusMarca.SelectedIndex = marcas is null || marcas.Status == 1 ? 0 : 1;

        cbAutorizada.IsChecked = marcas?.Autorizada == 1;

        GridModal.IsVisible = true;
        ModalMarca.IsVisible = true;
        ModalAparelho.IsVisible = false;
        ModalCidade.IsVisible = false;
        eMarca.Focus();
    }
    private async void OnSalvarGridMarca(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(eMarca.Text))
        {
            await DisplayAlertAsync("Atenção", "Informe o aparelho.", "OK");
            return;
        }

        int status = pStatusMarca.SelectedIndex == 1 ? 0 : 1;
        int autorizada = cbAutorizada.IsChecked ? 1 : 0;

        if (_marcasNovaEditar is null)
        {
            await _bancoLocal.SalvarMarcaAsync(new Marcas { Nome = eMarca.Text.Trim(), Status = status, Autorizada = autorizada });
        }
        else
        {
            _marcasNovaEditar.Nome = eMarca.Text.Trim();
            _marcasNovaEditar.Status = status;
            _marcasNovaEditar.Autorizada = status;
            await _bancoLocal.SalvarMarcaAsync(_marcasNovaEditar);
        }

        FecharGridModal();
        await ListaDadosAsync(false);
    }
    private async void OnAlterarStatusMarcaClicked(object sender, EventArgs e)
    {
        if (((Button)sender).BindingContext is not Marcas marca) return;

        bool ativar = marca.Status != 1;
        string acao = ativar ? "ativar" : "desativar";

        bool ok = await DisplayAlertAsync("Confirmar", $"Deseja {acao} \"{marca.Nome}\"?", "Sim", "Não");
        if (!ok) return;

        marca.Status = ativar ? 1 : 0;
        await _bancoLocal.SalvarMarcaAsync(marca);
        await ListaDadosAsync(false);
    }
    #endregion

    #region Metodos genericos
    private void OnCancelarGridModal(object sender, EventArgs e)
    {
        FecharGridModal();
    }

    private void FecharGridModal()
    {
        GridModal.IsVisible = false;
        _localidadesNovaEditar = null;
        _localidadesParaExcluir = null;
        _aparelhosNovaEditar = null;
    }
    #endregion
}