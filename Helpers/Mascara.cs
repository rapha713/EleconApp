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
        Moeda
    }

    public static class Mascara
    {
        private const string PadraoCpf = "###.###.###-##";
        private const string PadraoCnpj = "##.###.###/####-##";
        private const string PadraoTelefoneFixo = "(##) ####-####";
        private const string PadraoTelefoneCelular = "(##) #####-####";
        private const string PadraoData = "##/##/####";
        private const string PadraoMoeda = "R$ #.###,##";

        public static string SomenteDigitos(string? texto) =>
            string.IsNullOrEmpty(texto) ? string.Empty : new string(texto.Where(char.IsDigit).ToArray());

        public static string Aplicar(TipoMascara tipo, string? texto)
        {
            var d = SomenteDigitos(texto);

            return tipo switch
            {
                TipoMascara.Data => Padrao(PadraoData, d),
                TipoMascara.Moeda => Padrao(PadraoMoeda, d),
                TipoMascara.Cpf => Padrao(PadraoCpf, d),
                TipoMascara.Cnpj => Padrao(PadraoCnpj, d),
                TipoMascara.CpfCnpj => d.Length <= 11 ? Padrao(PadraoCpf, d) : Padrao(PadraoCnpj, d),
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
    }
}