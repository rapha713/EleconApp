using CadastroClientes.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace CadastroClientes.Data
{
    public class BancoLocal
    {
        private readonly SQLiteAsyncConnection _database;
        private bool _inicializado;

        public BancoLocal()
        {
            string caminho = Path.Combine(FileSystem.AppDataDirectory, "BancoLocal.db3"); //Caminho do banco de dados SQLite
            System.Diagnostics.Debug.WriteLine($"Banco SQLite: {caminho}");
            _database = new SQLiteAsyncConnection(caminho);
        }

        public async Task InicializarAsync()
        {
            if (_inicializado)
                return;

            await _database.CreateTableAsync<Cliente>();

            _inicializado = true;
        }
        
        public async Task<int> SalvarClienteAsync(Cliente cliente)
        {
            await InicializarAsync();

            if (cliente.Id == Guid.Empty)
                cliente.Id = Guid.NewGuid();

            //if (cliente.IdSincronizacao == Guid.Empty)
            //    cliente.IdSincronizacao = Guid.NewGuid();

            return await _database.InsertAsync(cliente);
        }

        public async Task<List<Cliente>> ObterClientesAsync()
        {
            await InicializarAsync();
            return await _database.Table<Cliente>().ToListAsync();
        }

        //public async Task<List<Cliente>> ObterClientesPendentesAsync()
        //{
        //    await InicializarAsync();

        //    return await _database.Table<Cliente>().Where(c => !c.Sincronizado).ToListAsync();
        //}

        //public async Task MarcarClienteSincronizadoAsync(Cliente cliente)
        //{
        //    await InicializarAsync();

        //    cliente.Sincronizado = true;

        //    await _database.UpdateAsync(cliente);
        //}

        public async Task AplicarAlteracoesAsync(IEnumerable<Cliente> inserir, IEnumerable<Cliente> atualizar, IEnumerable<Cliente> remover)
        {
            await InicializarAsync();

            await _database.RunInTransactionAsync(conn =>
            {
                foreach (var c in inserir) conn.Insert(c);
                foreach (var c in atualizar) conn.Update(c);
                foreach (var c in remover) conn.Delete(c);
            });
        }

        public async Task RemoverClienteAsync(Cliente cliente)
        {
            await InicializarAsync();
            await _database.DeleteAsync(cliente);
        }

        //public async Task ExcluirClienteAsync(Cliente cliente)
        //{
        //    await InicializarAsync();

        //    cliente.Excluido = 1;
        //    cliente.Sincronizado = false;
        //    cliente.DataAtualizacao = DateTime.Now;

        //    await _database.UpdateAsync(cliente);
        //}
    }
}
