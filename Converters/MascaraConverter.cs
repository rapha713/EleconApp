using System.Globalization;
using CadastroClientes.Helpers;

namespace CadastroClientes.Converters
{
    public class MascaraConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var tipo = parameter is string s && Enum.TryParse<TipoMascara>(s, true, out var t) ? t : TipoMascara.CpfCnpj;

            if (tipo == TipoMascara.Moeda)
            {
                if (value is null) return string.Empty;

                decimal valor = System.Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                long centavos = (long)Math.Round(valor * 100m);

                return Mascara.Aplicar(TipoMascara.Moeda, centavos.ToString(CultureInfo.InvariantCulture));
            }

            return Mascara.Aplicar(tipo, value?.ToString());
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}