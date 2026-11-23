using CadastroClientes.Views;

namespace CadastroClientes
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(ClientesPage), typeof(ClientesPage));

            Routing.RegisterRoute(nameof(CadastroClientePage),typeof(CadastroClientePage));

            Routing.RegisterRoute(nameof(OrdemServicoPage), typeof(OrdemServicoPage));

            Routing.RegisterRoute(nameof(EditarOsPage), typeof(EditarOsPage));

            Routing.RegisterRoute(nameof(ConfiguracaoPage), typeof(ConfiguracaoPage));
        }
    }
}
