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

        public List<FileInfo> GetLogFiles()
        {
            if (!Directory.Exists(_logFolderPath)) return new List<FileInfo>();

            var directoryInfo = new DirectoryInfo(_logFolderPath);
            return directoryInfo.GetFiles("*.log")
                                .OrderByDescending(f => f.LastWriteTime)
                                .ToList();
        }

        public string ReadLogFile(string fileName)
        {
            string filePath = Path.Combine(_logFolderPath, fileName);

            if (!File.Exists(filePath))
            {
                return "File log tidak ditemukan.";
            }

            try
            {
                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                return $"[Gagal membaca file log]: {ex.Message}";
            }
        }
    }
}