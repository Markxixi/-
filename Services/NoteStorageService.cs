using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using SmartNotes.Models;

namespace SmartNotes.Services
{
    public class NoteStorageService
    {
        private readonly string _dataDirectory;
        private readonly string _notesFilePath;
        private readonly string _settingsFilePath;
        private readonly JsonSerializerOptions _jsonOptions;

        public NoteStorageService()
        {
            _dataDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SmartNotes"
            );

            if (!Directory.Exists(_dataDirectory))
            {
                Directory.CreateDirectory(_dataDirectory);
            }

            _notesFilePath = Path.Combine(_dataDirectory, "notes.json");
            _settingsFilePath = Path.Combine(_dataDirectory, "settings.json");

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<List<Note>> LoadNotesAsync()
        {
            try
            {
                if (!File.Exists(_notesFilePath))
                {
                    return new List<Note>();
                }

                var json = await File.ReadAllTextAsync(_notesFilePath);
                var notes = JsonSerializer.Deserialize<List<Note>>(json, _jsonOptions);
                return notes ?? new List<Note>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载便签失败: {ex.Message}");
                return new List<Note>();
            }
        }

        public async Task SaveNotesAsync(List<Note> notes)
        {
            try
            {
                var json = JsonSerializer.Serialize(notes, _jsonOptions);
                await File.WriteAllTextAsync(_notesFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存便签失败: {ex.Message}");
                throw;
            }
        }

        public async Task<Dictionary<string, string>> LoadSettingsAsync()
        {
            try
            {
                if (!File.Exists(_settingsFilePath))
                {
                    return new Dictionary<string, string>();
                }

                var json = await File.ReadAllTextAsync(_settingsFilePath);
                var settings = JsonSerializer.Deserialize<Dictionary<string, string>>(json, _jsonOptions);
                return settings ?? new Dictionary<string, string>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载设置失败: {ex.Message}");
                return new Dictionary<string, string>();
            }
        }

        public async Task SaveSettingsAsync(Dictionary<string, string> settings)
        {
            try
            {
                var json = JsonSerializer.Serialize(settings, _jsonOptions);
                await File.WriteAllTextAsync(_settingsFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存设置失败: {ex.Message}");
                throw;
            }
        }

        public string DataDirectory => _dataDirectory;
    }
}
