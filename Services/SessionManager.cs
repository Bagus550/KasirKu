using KasirKu.Models;

namespace KasirKu.Services
{
    public static class SessionManager
    {
        public static Kasir? CurrentKasir { get; set; }
        public static KasirSession? CurrentSession { get; set; }

        public static bool IsAdmin => CurrentKasir != null &&
                                   CurrentKasir.Role != null &&
                                   CurrentKasir.Role.Equals("Admin", System.StringComparison.OrdinalIgnoreCase);

        public static bool HasActiveShift => CurrentSession != null && !CurrentSession.IsClosed;

        /// <summary>
        /// Menetapkan session kasir dan shift yang sedang aktif.
        /// </summary>
        public static void SetSession(Kasir kasir, KasirSession session)
        {
            CurrentKasir = kasir;
            CurrentSession = session;
        }

        public static void ClearSession()
        {
            CurrentKasir = null;
            CurrentSession = null;
        }
    }
}