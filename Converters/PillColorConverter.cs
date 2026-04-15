using System.Globalization;

namespace BiblioLinx.Converters
{
    public class PillColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var title = value?.ToString()?.ToUpper() ?? "";

            // Lógica simples: dependendo do nome da página ou grupo, ele retorna uma cor
            if (title.Contains("DTEF")) return Color.FromArgb("#2CB4F3"); // Azul
            if (title.Contains("ERRO")) return Color.FromArgb("#FF8C20"); // Laranja
            if (title.Contains("IA")) return Color.FromArgb("#A334E9");   // Roxo
            
            return Color.FromArgb("#3BC65E"); // Verde (padrão)
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}