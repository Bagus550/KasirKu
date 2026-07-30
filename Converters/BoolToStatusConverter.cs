using System;
using System.Globalization;
using System.Windows.Data;

namespace KasirKu.Converters
{
    public class BoolToStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isClosed)
            {
                // Jika IsClosed == true berarti Shift Sudah Selesai/Tutup, jika false berarti Masih Aktif
                return isClosed ? "🔴 Selesai" : "🟢 Aktif";
            }
            return "-";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}