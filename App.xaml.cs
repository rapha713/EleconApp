using CadastroClientes.Data;
using System.Diagnostics;

namespace CadastroClientes
{
    public partial class App : Application
    {
        public App(BancoLocal bancoLocal)
        {
            InitializeComponent();

            _ = InicializarBancoAsync(bancoLocal);
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }

        private static async Task InicializarBancoAsync(BancoLocal bancoLocal)
        {
            try
            {
                await bancoLocal.InicializarAsync();

                Debug.WriteLine("Banco SQLite inicializado com sucesso.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao inicializar o SQLite: {ex}");
            }
        }
    }
}