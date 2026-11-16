using CadastroClientes.Helpers;

namespace CadastroClientes.Behaviors
{
    public class MascaraBehavior : Behavior<Entry>
    {
        public static readonly BindableProperty TipoProperty =
            BindableProperty.Create(nameof(Tipo), typeof(TipoMascara), typeof(MascaraBehavior), TipoMascara.CpfCnpj);

        public TipoMascara Tipo
        {
            get => (TipoMascara)GetValue(TipoProperty);
            set => SetValue(TipoProperty, value);
        }

        private bool _aplicando;

        protected override void OnAttachedTo(Entry entry)
        {
            base.OnAttachedTo(entry);
            entry.TextChanged += OnTextChanged;
        }

        protected override void OnDetachingFrom(Entry entry)
        {
            entry.TextChanged -= OnTextChanged;
            base.OnDetachingFrom(entry);
        }

        private void OnTextChanged(object? sender, TextChangedEventArgs e)
        {
            if (_aplicando || sender is not Entry entry)
                return;

            if (Mascara.Aplicar(Tipo, e.NewTextValue) == e.NewTextValue)
                return;

            entry.Dispatcher.Dispatch(() =>
            {
                _aplicando = true;
                try
                {
                    var formatado = Mascara.Aplicar(Tipo, entry.Text);

                    if (formatado != entry.Text)
                    {
                        entry.Text = formatado;
                        entry.CursorPosition = Math.Min(formatado.Length, entry.Text?.Length ?? 0);
                    }
                }
                catch
                {
                }
                finally
                {
                    _aplicando = false;
                }
            });
        }
    }
}