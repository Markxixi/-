using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartNotes.Models;
using SmartNotes.Services;

namespace SmartNotes.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly DoubaoApiService _aiService;
        private readonly NoteStorageService _storageService;

        // 保存所有便签的原始数据
        private List<Note> _allNotes = new();

        [ObservableProperty]
        private ObservableCollection<Note> _notes = new();

        [ObservableProperty]
        private Note? _selectedNote;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private string _selectedCategory = "全部";

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _statusMessage = "就绪";

        [ObservableProperty]
        private string _apiKey = string.Empty;

        [ObservableProperty]
        private string _apiEndpoint = "https://ark.cn-beijing.volces.com/api/v3";

        [ObservableProperty]
        private string _aiModel = "ep-20260405132959-vq7kd";

        [ObservableProperty]
        private bool _isApiConfigured;

        [ObservableProperty]
        private bool _isSettingsPanelVisible;

        [ObservableProperty]
        private string _aiPrompt = string.Empty;

        [ObservableProperty]
        private bool _isAIGenerating;

        public ObservableCollection<string> Categories { get; } = new()
        {
            "全部", "默认", "工作", "生活", "学习", "其他"
        };

        public MainViewModel()
        {
            _aiService = new DoubaoApiService();
            _storageService = new NoteStorageService();
        }

        public async Task InitializeAsync()
        {
            await LoadSettingsAsync();
            await LoadNotesAsync();

            if (_allNotes.Count == 0)
            {
                var welcomeNote = new Note
                {
                    Title = "欢迎使用智能便签",
                    Content = "👋 欢迎使用智能便签应用！\n\n" +
                             "✨ 功能特点：\n" +
                             "• 创建、编辑、删除便签\n" +
                             "• 使用 AI 制定计划\n" +
                             "• 便签分类管理\n" +
                             "• 本地数据存储\n\n" +
                             "💡 使用提示：\n" +
                             "点击「AI 助手」按钮，让豆包大模型帮您制定计划！",
                    Category = "默认"
                };
                _allNotes.Add(welcomeNote);
                await SaveNotesAsync();
                RefreshFilteredNotes();
            }
        }

        private async Task LoadNotesAsync()
        {
            try
            {
                IsLoading = true;
                var notes = await _storageService.LoadNotesAsync();
                _allNotes = notes;
                RefreshFilteredNotes();
                StatusMessage = $"已加载 {_allNotes.Count} 条便签";
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载失败: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SaveNotesAsync()
        {
            try
            {
                await _storageService.SaveNotesAsync(_allNotes);
            }
            catch (Exception ex)
            {
                StatusMessage = $"保存失败: {ex.Message}";
            }
        }

        // 刷新显示的便签列表（根据分类和搜索过滤）
        private void RefreshFilteredNotes()
        {
            var filtered = _allNotes.Where(n =>
            {
                var matchesCategory = SelectedCategory == "全部" || n.Category == SelectedCategory;
                var matchesSearch = string.IsNullOrEmpty(SearchText) ||
                                  n.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                                  n.Content.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
                return matchesCategory && matchesSearch;
            }).OrderByDescending(n => n.IsPinned).ThenByDescending(n => n.ModifiedTime);

            Notes.Clear();
            foreach (var note in filtered)
            {
                Notes.Add(note);
            }
        }

        private async Task LoadSettingsAsync()
        {
            var settings = await _storageService.LoadSettingsAsync();
            if (settings.TryGetValue("apiKey", out var apiKey) && !string.IsNullOrEmpty(apiKey))
            {
                ApiKey = apiKey;
                if (settings.TryGetValue("apiEndpoint", out var endpoint))
                {
                    ApiEndpoint = endpoint;
                }
                if (settings.TryGetValue("aiModel", out var model))
                {
                    AiModel = model;
                }
                ConfigureApi();
            }
        }

        private async Task SaveSettingsAsync()
        {
            var settings = new System.Collections.Generic.Dictionary<string, string>
            {
                ["apiKey"] = ApiKey,
                ["apiEndpoint"] = ApiEndpoint,
                ["aiModel"] = AiModel
            };
            await _storageService.SaveSettingsAsync(settings);
        }

        private void ConfigureApi()
        {
            if (!string.IsNullOrEmpty(ApiKey))
            {
                _aiService.Configure(ApiKey, ApiEndpoint, AiModel);
                IsApiConfigured = true;
                StatusMessage = "AI 服务已配置";
            }
            else
            {
                IsApiConfigured = false;
            }
        }

        [RelayCommand]
        private async Task CreateNote()
        {
            var newNote = new Note
            {
                Title = "新便签",
                Content = "",
                Category = SelectedCategory == "全部" ? "默认" : SelectedCategory
            };
            _allNotes.Add(newNote);
            RefreshFilteredNotes();
            SelectedNote = newNote;
            await SaveNotesAsync();
            StatusMessage = "已创建新便签";
        }

        [RelayCommand]
        private async Task SaveNote()
        {
            if (SelectedNote != null)
            {
                SelectedNote.ModifiedTime = DateTime.Now;
                await SaveNotesAsync();
                RefreshFilteredNotes();
                StatusMessage = "已保存";
            }
        }

        [RelayCommand]
        private async Task DeleteNote()
        {
            if (SelectedNote != null)
            {
                var result = MessageBox.Show(
                    $"确定要删除便签「{SelectedNote.Title}」吗？",
                    "确认删除",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    var noteToDelete = SelectedNote;
                    _allNotes.Remove(noteToDelete);
                    RefreshFilteredNotes();
                    SelectedNote = Notes.FirstOrDefault();
                    await SaveNotesAsync();
                    StatusMessage = "已删除便签";
                }
            }
        }

        [RelayCommand]
        private async Task TogglePin()
        {
            if (SelectedNote != null)
            {
                SelectedNote.IsPinned = !SelectedNote.IsPinned;
                RefreshFilteredNotes();
                await SaveNotesAsync();
                StatusMessage = SelectedNote.IsPinned ? "已置顶" : "已取消置顶";
            }
        }

        [RelayCommand]
        private void ToggleSettings()
        {
            IsSettingsPanelVisible = !IsSettingsPanelVisible;
        }

        [RelayCommand]
        private async Task SaveApiSettings()
        {
            ConfigureApi();
            await SaveSettingsAsync();
            IsSettingsPanelVisible = false;
            StatusMessage = "API 设置已保存";
        }

        [RelayCommand]
        private void OpenAIAssistant()
        {
            AiPrompt = string.Empty;
            IsAIGenerating = true;
        }

        [RelayCommand]
        private void CancelAIGeneration()
        {
            IsAIGenerating = false;
            AiPrompt = string.Empty;
        }

        [RelayCommand]
        private async Task GeneratePlan()
        {
            if (string.IsNullOrWhiteSpace(AiPrompt))
            {
                MessageBox.Show("请输入您的需求描述", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!IsApiConfigured)
            {
                MessageBox.Show("请先在设置中配置豆包 API 密钥", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = "正在生成计划...";

                var plan = await _aiService.GeneratePlanAsync(AiPrompt);

                var planNote = new Note
                {
                    Title = $"计划 - {DateTime.Now:MM月dd日 HH:mm}",
                    Content = $"📋 需求描述：\n{AiPrompt}\n\n📌 生成的计划：\n{plan}",
                    Category = "工作",
                    IsAIGenerated = true,
                    AiPrompt = AiPrompt
                };

                _allNotes.Add(planNote);
                RefreshFilteredNotes();
                SelectedNote = planNote;
                await SaveNotesAsync();

                IsAIGenerating = false;
                AiPrompt = string.Empty;
                StatusMessage = "计划已生成并保存";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"生成计划失败：\n{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusMessage = "生成失败";
            }
            finally
            {
                IsLoading = false;
            }
        }

        partial void OnSearchTextChanged(string value)
        {
            RefreshFilteredNotes();
        }

        partial void OnSelectedCategoryChanged(string value)
        {
            RefreshFilteredNotes();
        }

        [RelayCommand]
        private async Task ChangeCategory(string category)
        {
            if (SelectedNote != null)
            {
                SelectedNote.Category = category;
                await SaveNotesAsync();
                RefreshFilteredNotes();
                StatusMessage = $"已移动到「{category}」分类";
            }
        }
    }
}
