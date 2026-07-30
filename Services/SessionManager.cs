using KasirKu.Models;

namespace KasirKu.Services
{
    public static class SessionManager
    {
        public static Kasir? CurrentKasir { get; private set; }
        public static KasirSession? CurrentSession { get; private set; }

        public static bool IsLoggedIn => CurrentKasir != null && CurrentSession != null;

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