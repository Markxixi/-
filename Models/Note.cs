using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SmartNotes.Models
{
    public partial class Note : ObservableObject
    {
        [ObservableProperty]
        private string _id = Guid.NewGuid().ToString();

        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private string _content = string.Empty;

        [ObservableProperty]
        private DateTime _createdTime = DateTime.Now;

        [ObservableProperty]
        private DateTime _modifiedTime = DateTime.Now;

        [ObservableProperty]
        private string _category = "默认";

        [ObservableProperty]
        private bool _isPinned;

        [ObservableProperty]
        private bool _isAIGenerated;

        [ObservableProperty]
        private string _aiPrompt = string.Empty;

        public Note() { }

        public Note(string title, string content, string category = "默认")
        {
            _title = title;
            _content = content;
            _category = category;
        }

        public Note Clone()
        {
            return new Note
            {
                Id = this.Id,
                Title = this.Title,
                Content = this.Content,
                CreatedTime = this.CreatedTime,
                ModifiedTime = this.ModifiedTime,
                Category = this.Category,
                IsPinned = this.IsPinned,
                IsAIGenerated = this.IsAIGenerated,
                AiPrompt = this.AiPrompt
            };
        }
    }
}
