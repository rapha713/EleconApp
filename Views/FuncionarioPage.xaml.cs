using CadastroClientes.Data;
using CadastroClientes.Models;
using CadastroClientes.Services;
using System.Globalization;

namespace CadastroClientes.Views;

public partial class FuncionarioPage : ContentPage
{
    private const double LarguraMinimaDesktop = 700;

    private readonly Funcionarios _funcionarios;
    private readonly BancoLocal _bancoLocal;
    private readonly ApiService _apiService;
    //private readonly SincronizacaoService _sincronizacao;
    private List<Funcionarios> _todosFuncionarios = new();
    private List<Funcionarios> _funcionariosFiltrados = new();
    private Funcionarios? _funcionarioNovaEditar;
    private Funcionarios? _funcionarioParaExcluir;
    public FuncionarioPage(BancoLocal bancoLocal, ApiService apiService)
    {
        InitializeComponent();
        _bancoLocal = bancoLocal;
        _apiService = apiService;
        //_sincronizacao = sincronizacao;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await ListaFuncionarioAsync(online: false);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
    }

    private async void btnVoltarClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ClientesPage));
    }

    private async void OnPaginaInicialClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ClientesPage));
    }

    private async void OnOrdemServicoClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(OrdemServicoPage));
    }

    private async void OnConfiguracaoClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ConfiguracaoPage));
    }

    private async void OnSairClicked(object sender, EventArgs e)
    {
        bool ok = await DisplayAlertAsync("Sair", "Deseja realmente fechar o programa?", "Sim", "Não");
        if (!ok) return;

        Application.Current?.Quit();
    }

    private async Task ListaFuncionarioAsync(bool online)
    {
        try
        {
            _todosFuncionarios = (await _bancoLocal.ObterFuncionariosAsync())
                .OrderByDescending(c => c.Nome).ToList();

            AplicarFiltro();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", $"Não foi possível carregar os clientes: {ex.Message}", "OK");
        }
    }

    private void AplicarFiltro()
    {
        //string texto = (eFiltros.Text ?? string.Empty).Trim();
        //string? campo = pFiltros.SelectedItem as string;
        //string? exibicao = pFiltrosAndamento.SelectedItem as string;

        //var cmp = StringComparison.CurrentCultureIgnoreCase;
        IEnumerable<Funcionarios> lista = _todosFuncionarios;

        //if (!string.IsNullOrEmpty(texto) && campo is not null)
        //{
        //    lista = campo switch
        //    {
        //        "OS" => lista.Where(c => c.OS.ToString().Contains(texto)),
        //        "Nome" => lista.Where(c => (c.Nome ?? "").Contains(texto, cmp)),
        //        "Telefone" => lista.Where(c => SomenteDigitos(c.Telefone).Contains(SomenteDigitos(texto))),
        //        "Aparelho" => lista.Where(c => c.AparelhoTexto.Contains(texto, cmp)),
        //        _ => lista
        //    };
        //}

        //if (exibicao is null || exibicao == "Todos")
        //    lista = lista.Where(c => c.Andamento != "Entregue");
        //else
        //    lista = lista.Where(c => string.Equals(c.Andamento, exibicao, cmp));

        _funcionariosFiltrados = lista.ToList();
        cvFuncionario.ItemsSource = _funcionariosFiltrados;
    }

    private void OnAdicionarFuncionarioClicked(object sender, EventArgs e) => AbrirGridModal();

    private void OnEditarFuncionarioClicked(object sender, EventArgs e)
    {
        if (((Button)sender).BindingContext is Funcionarios funcionario)
            AbrirGridModal(funcionario);
    }

    private void AbrirGridModal(Funcionarios? funcionario = null)
    {
        _funcionarioNovaEditar = funcionario;

        lTituloModalFuncionario.Text = funcionario is null ? "Adicionar Funcionário" : "Editar Funcionário";
        eNome.Text = funcionario?.Nome ?? string.Empty;
        eFuncao.Text = funcionario?.Funcao ?? string.Empty;
        eDocumento.Text = funcionario?.Documento.ToString() ?? string.Empty;
        pStatus.SelectedIndex = funcionario is null || funcionario.Status == 1 ? 0 : 1;

        GridModal.IsVisible = true;
        eNome.Focus();
    }

    private async void OnSalvarGridModal(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(eNome.Text))
        {
            await DisplayAlertAsync("Atenção", "Informe o nome do funcionário.", "OK");
            return;
        }
        if (string.IsNullOrWhiteSpace(eFuncao.Text))
        {
            await DisplayAlertAsync("Atenção", "Informe a função do funcionário.", "OK");
            return;
        }

        try
        {
            var funcionario = _funcionarioNovaEditar ?? new Funcionarios();

            funcionario.Nome = eNome.Text.Trim();
            funcionario.Funcao = eFuncao.Text.Trim();
            funcionario.Documento = SomenteDigitos(eDocumento.Text);
            funcionario.Status = pStatus.SelectedIndex == 0 ? 1 : 0;

            await _bancoLocal.SalvarFuncionarioAsync(funcionario);

            FecharGridModal();
            await ListaFuncionarioAsync(false);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", $"Não foi possível salvar: {ex.Message}", "OK");
        }
    }

    private static string SomenteDigitos(string? s) =>
        new string((s ?? "").Where(char.IsDigit).ToArray());

    private async Task CarregarFuncionarioAsync()
    {
        IEnumerable<Funcionarios> lista = _todosFuncionarios;

        _todosFuncionarios.Clear();
        
        foreach (var p in await _bancoLocal.ObterNovosFuncionariosAsync(_funcionarios.Id))
            _todosFuncionarios.Add(p);

        _funcionariosFiltrados = lista.ToList();
        cvFuncionario.ItemsSource = _funcionariosFiltrados;
    }

    private async void OnAlterarStatusFuncionarioClicked(object sender, EventArgs e)
    {
        if (((Button)sender).BindingContext is not Funcionarios funcionario) return;

        bool ativar = funcionario.Status != 1;
        string acao = ativar ? "ativar" : "desativar";

        bool ok = await DisplayAlertAsync("Confirmar", $"Deseja {acao} \"{funcionario.Nome}\"?", "Sim", "Não");
        if (!ok) return;

        funcionario.Status = ativar ? 1 : 0;
        await _bancoLocal.SalvarFuncionarioAsync(funcionario);
        await ListaFuncionarioAsync(false);
    }

    private void OnCancelarGridModal(object sender, EventArgs e)
    {
        FecharGridModal();
    }

    private void FecharGridModal()
    {
        GridModal.IsVisible = false;
        _funcionarioNovaEditar = null;
    }
}