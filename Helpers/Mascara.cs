using System.Globalization;
using System.Text;

namespace CadastroClientes.Helpers
{
    public enum TipoMascara
    {
        Cpf,
        Cnpj,
        CpfCnpj,
        Telefone,
        Data,
        Moeda,
        CEP
    }

    public static class Mascara
    {
        private const string PadraoCpf = "###.###.###-##";
        private const string PadraoTelefoneFixo = "(##) ####-####";
        private const string PadraoTelefoneCelular = "(##) #####-####";
        private const string PadraoData = "##/##/####";
        private const string PadraoCEP = "#####-###";
        private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

        public static string SomenteDigitos(string? texto) =>
            string.IsNullOrEmpty(texto) ? string.Empty : new string(texto.Where(char.IsDigit).ToArray());

        public static string Aplicar(TipoMascara tipo, string? texto)
        {
            var d = SomenteDigitos(texto);

            return tipo switch
            {
                TipoMascara.Cpf => Padrao(PadraoCpf, d),
                TipoMascara.Data => Padrao(PadraoData, d),
                TipoMascara.Moeda => FormatarMoeda(d),
                TipoMascara.CEP => Padrao(PadraoCEP, d),
                TipoMascara.Telefone => d.Length <= 10
                    ? Padrao(PadraoTelefoneFixo, d)
                    : Padrao(PadraoTelefoneCelular, d),
                _ => d
            };
        }

        private static string Padrao(string padrao, string digitos)
        {
            var sb = new StringBuilder();
            int i = 0;

            foreach (var c in padrao)
            {
                if (i >= digitos.Length)
                    break;

                if (c == '#')
                    sb.Append(digitos[i++]);
                else
                    sb.Append(c);
            }

            return sb.ToString();
        }

        private static string FormatarMoeda(string digitos)
        {
            if (string.IsNullOrEmpty(digitos)) return string.Empty;

            if (digitos.Length > 12)
                digitos = digitos[..12];

            decimal valor = long.Parse(digitos) / 100m;
            return valor.ToString("C2", PtBr);
        }
    }
    public static class Formatacao
    {
        public static string SimOuTraco(bool? valor) => valor == true ? "Sim" : "-";
    }
}