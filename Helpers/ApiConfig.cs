namespace CadastroClientes.Helpers
{
    public static class ApiConfig
    {
#if DEBUG
#if ANDROID
        public const string BaseUrl = "http://10.0.2.2:5183/";
#else
        public const string BaseUrl = "http://localhost:5183/";
#endif

#else
        public const string BaseUrl = "https://restrito.consorcioapice.com.br/tstMaui/";
#endif
    }
}