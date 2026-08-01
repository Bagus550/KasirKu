using KasirKu.Models;
using System;

namespace KasirKu.Services
{
    public static class SessionManager
    {
        public static Kasir? CurrentKasir { get; private set; }
        public static KasirSession? CurrentSession { get; private set; }

        public static bool IsAdmin => CurrentKasir != null &&
                                   CurrentKasir.Role != null &&
                                   CurrentKasir.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase);

        public static bool HasActiveShift => CurrentSession != null && !CurrentSession.IsClosed;

        public static event EventHandler? SessionCleared;

        public static void SetSession(Kasir kasir, KasirSession session)
        {
            CurrentKasir = kasir;
            CurrentSession = session;
        }

        public static void ClearSession()
        {
            CurrentKasir = null;
            CurrentSession = null;

            SessionCleared?.Invoke(null, EventArgs.Empty);
        }
    }
}