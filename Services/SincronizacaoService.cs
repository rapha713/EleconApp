//using System.Globalization;
//using CadastroClientes.Data;
//using CadastroClientes.Models;

//namespace CadastroClientes.Services;

//public class SincronizacaoService
//{
//    private const string ChaveUltimaSincronizacao = "ultima_sincronizacao";

//    private readonly BancoLocal _bancoLocal;
//    private readonly ApiService _apiService;
//    private readonly SemaphoreSlim _trava = new(1, 1);  

//    public SincronizacaoService(BancoLocal bancoLocal, ApiService apiService)
//    {
//        _bancoLocal = bancoLocal;
//        _apiService = apiService;
//    }

//    public async Task<bool> SincronizarAsync()
//    {
//        await _trava.WaitAsync();

//        try
//        {
//            if (!await _apiService.ObterConexaoAsync())
//                return false;

//            await EnviarPendentesAsync();
//            await ReceberAlteracoesAsync();

//            return true;
//        }
//        catch (Exception ex)
//        {
//            System.Diagnostics.Debug.WriteLine($"Erro na sincronização: {ex.Message}");
//            return false;
//        }
//        finally
//        {
//            _trava.Release();
//        }
//    }

//    private async Task EnviarPendentesAsync()
//    {
//        foreach (var cliente in await _bancoLocal.ObterClientesPendentesAsync())
//        {
//            try
//            {
//                if (!await _apiService.SincronizarClienteAsync(cliente))
//                    continue;

//                if (cliente.Excluido == 1)
//                    await _bancoLocal.RemoverClienteAsync(cliente);
//                else
//                    await _bancoLocal.MarcarClienteSincronizadoAsync(cliente);
//            }
//            catch (Exception ex)
//            {
//                System.Diagnostics.Debug.WriteLine($"Falha ao enviar {cliente.Nome}: {ex.Message}");
//            }
//        }
//    }

//    private async Task ReceberAlteracoesAsync()
//    {
//        var todosLocais = await _bancoLocal.ObterClientesAsync();
//        DateTime? desde = todosLocais.Count == 0 ? null : ObterUltimaSincronizacao();

//        var alteracoes = await _apiService.ObterAlteracoesAsync(desde);

//        if (alteracoes.Count == 0)
//            return;

//        var locais = todosLocais.Where(c => c.IdSincronizacao != Guid.Empty)
//            .GroupBy(c => c.IdSincronizacao).ToDictionary(g => g.Key, g => g.First());

//        var inserir = new List<Cliente>();
//        var atualizar = new List<Cliente>();
//        var remover = new List<Cliente>();

//        foreach (var dto in alteracoes)
//        {
//            if (dto.IdSincronizacao is not Guid id || id == Guid.Empty)
//            {
//                System.Diagnostics.Debug.WriteLine($"Ignorado (sem IdSincronizacao): {dto.Nome}");
//                continue;
//            }

//            locais.TryGetValue(id, out var local);

//            if (dto.Excluido)
//            {
//                if (local != null)
//                    remover.Add(local);
//                continue;
//            }

//            if (local == null)
//            {
//                var novo = new Cliente { IdCliente = Guid.NewGuid(), IdSincronizacao = id };
//                Aplicar(novo, dto);
//                inserir.Add(novo);
//            }
//            else if (local.Sincronizado)
//            {
//                Aplicar(local, dto);
//                atualizar.Add(local);
//            }
//        }

//        await _bancoLocal.AplicarAlteracoesAsync(inserir, atualizar, remover);

//        var maior = alteracoes.Max(a => a.DataAtualizacao);
//        if (maior != null)
//            SalvarUltimaSincronizacao(maior.Value);
//    }

//    private static void Aplicar(Cliente c, ClienteApi d)
//    {
//        c.Nome = d.Nome;
//        c.TipoPessoa = d.TipoPessoa;
//        c.Documento = d.Documento ?? string.Empty;
//        c.Email = d.Email;
//        c.Telefone = d.Telefone ?? string.Empty;
//        c.DataNascimento = d.DataNascimento ?? DateTime.MinValue;
//        c.Endereco = d.Endereco;
//        c.Bairro = d.Bairro;
//        c.UF = d.UF;
//        c.Cidade = d.Cidade ?? string.Empty;
//        c.Profissao = d.Profissao;
//        c.EstadoCivil = d.EstadoCivil;
//        c.DataCriacao = d.DataCriacao ?? c.DataCriacao;
//        c.DataAtualizacao = d.DataAtualizacao;
//        c.Excluido = 0;
//        c.Sincronizado = true;
//    }

//    private static DateTime? ObterUltimaSincronizacao()
//    {
//        string texto = Preferences.Default.Get(ChaveUltimaSincronizacao, string.Empty);

//        return DateTime.TryParse(texto, null, DateTimeStyles.RoundtripKind, out var data)
//            ? data
//            : null;
//    }

//    private static void SalvarUltimaSincronizacao(DateTime data) =>
//        Preferences.Default.Set(ChaveUltimaSincronizacao, data.ToString("O"));
//}