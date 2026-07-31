using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KasirKu.Services
{
    public interface ILoggerService
    {
        void LogError(Exception ex, string context = "");
        void LogInfo(string message);
        List<FileInfo> GetLogFiles();
        string ReadLogFile(string fileName);
    }

    public class LoggerService : ILoggerService
    {
        private readonly string _logFolderPath;

        public LoggerService()
        {
            _logFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            if (!Directory.Exists(_logFolderPath))
            {
                Directory.CreateDirectory(_logFolderPath);
            }
        }

        public void LogError(Exception ex, string context = "")
        {
            try
            {
                string fileName = $"error_{DateTime.Now:yyyyMMdd}.log";
                string filePath = Path.Combine(_logFolderPath, fileName);

                string logContent = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR {(string.IsNullOrEmpty(context) ? "" : $"[{context}]")}\n" +
                                   $"Message: {ex.Message}\n" +
                                   $"StackTrace: {ex.StackTrace}\n" +
                                   $"{new string('-', 60)}\n\n";

                File.AppendAllText(filePath, logContent);
            }
            catch { }
        }

        public void LogInfo(string message)
        {
            try
            {
                string fileName = $"info_{DateTime.Now:yyyyMMdd}.log";
                string filePath = Path.Combine(_logFolderPath, fileName);

                string logContent = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] INFO: {message}\n";
                File.AppendAllText(filePath, logContent);
            }
            catch { }
        }

        /// <summary>
        /// Mengambil daftar semua file log di direktori Logs (diurutkan dari yang terbaru).
        /// </summary>
        public List<FileInfo> GetLogFiles()
        {
            if (!Directory.Exists(_logFolderPath)) return new List<FileInfo>();

            var directoryInfo = new DirectoryInfo(_logFolderPath);
            return directoryInfo.GetFiles("*.log")
                                .OrderByDescending(f => f.LastWriteTime)
                                .ToList();
        }

        /// <summary>
        /// Membaca isi file log berdasarkan nama file.
        /// </summary>
        public string ReadLogFile(string fileName)
        {
            string filePath = Path.Combine(_logFolderPath, fileName);
            if (File.Exists(filePath))
            {
                return File.ReadAllText(filePath);
            }
            return "File log tidak ditemukan.";
        }
    }
}