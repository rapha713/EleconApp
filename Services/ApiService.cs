using CadastroClientes.Models;
using System.Net.Http.Json;

namespace CadastroClientes.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> ObterConexaoAsync()
        {
            var resposta = await _httpClient.GetAsync("api/conexao");

            return resposta.IsSuccessStatusCode;
        }

        //public async Task<bool> CadastrarClienteAsync(Cliente cliente)
        //{
            //var dados = new
            //{
            //    nome = cliente.Nome,
            //    tipoPessoa = cliente.TipoPessoa,
            //    documento = cliente.Documento,
            //    email = cliente.Email,
            //    telefone = cliente.Telefone,
            //    dataNascimento = cliente.DataNascimento,
            //    endereco = cliente.Endereco,
            //    bairro = cliente.Bairro,
            //    uf = cliente.UF,
            //    cidade = cliente.Cidade,
            //    profissao = cliente.Profissao,
            //    estadoCivil = cliente.EstadoCivil,
            //    excluido = false
            //};

            //var resposta = await _httpClient.PostAsJsonAsync("api/clientes", dados);

            //string conteudo = await resposta.Content.ReadAsStringAsync();

            //if (!resposta.IsSuccessStatusCode)
            //{
            //    throw new Exception($"Erro HTTP {(int)resposta.StatusCode}: {conteudo}");
            //}

            //return true;
        //}

        public async Task<bool> AtualizarClienteAsync(int id, Cliente cliente)
        {
            var resposta = await _httpClient.PutAsJsonAsync($"api/clientes/{id}", cliente);

            return resposta.IsSuccessStatusCode;
        }

        public async Task<bool> ExcluirClienteAsync(int id)
        {
            var resposta = await _httpClient.DeleteAsync($"api/clientes/{id}");

            return resposta.IsSuccessStatusCode;
        }

        public async Task<List<ClienteApi>> ObterAlteracoesAsync(DateTime? desde)
        {
            string url = "api/clientes/sincronizacao";

            if (desde != null)
                url += $"?desde={Uri.EscapeDataString(desde.Value.ToString("O"))}";

            var resposta = await _httpClient.GetAsync(url);

            if (!resposta.IsSuccessStatusCode)
            {
                string conteudo = await resposta.Content.ReadAsStringAsync();
                throw new Exception($"Erro HTTP {(int)resposta.StatusCode} em {url}: {conteudo}");
            }

            return await resposta.Content.ReadFromJsonAsync<List<ClienteApi>>()
                   ?? new List<ClienteApi>();
        }

        //public async Task<bool> SincronizarClienteAsync(Cliente cliente)
        //{
        //    var dados = new
        //    {
        //        idSincronizacao = cliente.IdSincronizacao,
        //        nome = cliente.Nome,
        //        tipoPessoa = cliente.TipoPessoa,
        //        documento = cliente.Documento,
        //        email = cliente.Email,
        //        telefone = cliente.Telefone,
        //        dataNascimento = cliente.DataNascimento,
        //        endereco = cliente.Endereco,
        //        bairro = cliente.Bairro,
        //        uf = cliente.UF,
        //        cidade = cliente.Cidade,
        //        profissao = cliente.Profissao,
        //        estadoCivil = cliente.EstadoCivil,
        //        excluido = cliente.Excluido == 1
        //    };

        //    var resposta = await _httpClient.PostAsJsonAsync("api/clientes/sincronizar", dados);

        //    return resposta.IsSuccessStatusCode;
        //}
    }
}