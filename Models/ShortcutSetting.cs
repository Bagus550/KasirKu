using System.Windows.Input;

namespace KasirKu.Models
{
    public class ShortcutSetting
    {
        public Key TambahBarang { get; set; } = Key.Enter;
        public Key HoldTransaksi { get; set; } = Key.F2;
        public Key ResumeTransaksi { get; set; } = Key.F3;
        public Key BatalTransaksi { get; set; } = Key.F4;
        public Key ProsesBayar { get; set; } = Key.F12;
        public Key FokusPencarian { get; set; } = Key.F1;
    }
}