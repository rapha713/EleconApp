using CadastroClientes.Data;
using CadastroClientes.Views;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using CadastroClientes.Helpers;
using CadastroClientes.Services;

namespace CadastroClientes
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });


            ConfigurarEntryInicioAoSairDoFoco();

#if DEBUG
            builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<BancoLocal>();
            builder.Services.AddTransient<CadastroClientePage>();
            builder.Services.AddTransient<ClientesPage>();
            builder.Services.AddTransient<ConfiguracaoPage>();
            builder.Services.AddTransient<EditarOsPage>();
            builder.Services.AddTransient<FuncionarioPage>();
            builder.Services.AddTransient<OrdemServicoPage>();
            builder.Services.AddTransient<TecnicoPage>();
            //builder.Services.AddSingleton<SincronizacaoService>();
            builder.Services.AddSingleton<HttpClient>(serviceProvider =>
            {
                return new HttpClient
                {
                    BaseAddress = new Uri(ApiConfig.BaseUrl),
                    Timeout = TimeSpan.FromSeconds(30)
                };
            });

            builder.Services.AddTransient<ApiService>();
            builder.Services.AddSingleton<RelatorioService>();
            return builder.Build();
        }

        /// <summary>
        /// Ao perder o foco, todo Entry volta a exibir o início do texto
        /// (ex.: "Matheus Henriq" em vez de "theus Henrique").
        /// </summary>
        private static void ConfigurarEntryInicioAoSairDoFoco()
        {
            EntryHandler.Mapper.AppendToMapping("InicioAoSairDoFoco", (handler, view) =>
            {
#if WINDOWS
                handler.PlatformView.LostFocus += (s, e) =>
                {
                    var textBox = handler.PlatformView;
                    textBox.Select(0, 0);

                    var scrollViewer = EncontrarScrollViewer(textBox);
                    scrollViewer?.ChangeView(0, null, null, true);
                };
#elif ANDROID
                handler.PlatformView.FocusChange += (s, e) =>
                {
                    if (!e.HasFocus)
                        handler.PlatformView.SetSelection(0);
                };
#elif IOS || MACCATALYST
                handler.PlatformView.EditingDidEnd += (s, e) =>
                {
                    var inicio = handler.PlatformView.BeginningOfDocument;
                    handler.PlatformView.SelectedTextRange =
                        handler.PlatformView.GetTextRange(inicio, inicio);
                };
#endif
            });
        }

#if WINDOWS
        private static Microsoft.UI.Xaml.Controls.ScrollViewer? EncontrarScrollViewer(Microsoft.UI.Xaml.DependencyObject pai)
        {
            int total = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(pai);
            for (int i = 0; i < total; i++)
            {
                var filho = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(pai, i);

                if (filho is Microsoft.UI.Xaml.Controls.ScrollViewer sv)
                    return sv;

                var encontrado = EncontrarScrollViewer(filho);
                if (encontrado != null)
                    return encontrado;
            }
            return null;
        }
#endif
    }
}