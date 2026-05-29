using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RTBrowser.Core
{
    public sealed class BrowserTab
        : INotifyPropertyChanged
    {
        private string _title =
            "New Tab";

        private string _url =
            "about:blank";

        private bool _isActive;

        private bool _isLoading;

        public event PropertyChangedEventHandler?
            PropertyChanged;

        public Guid Id { get; init; } =
            Guid.NewGuid();

        public DateTime CreatedAt { get; init; } =
            DateTime.UtcNow;

        public string Title
        {
            get => _title;

            set
            {
                if (_title == value)
                {
                    return;
                }

                _title = value;

                OnPropertyChanged();
            }
        }

        public string Url
        {
            get => _url;

            set
            {
                if (_url == value)
                {
                    return;
                }

                _url = value;

                OnPropertyChanged();
            }
        }

        public bool IsActive
        {
            get => _isActive;

            set
            {
                if (_isActive == value)
                {
                    return;
                }

                _isActive = value;

                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;

            set
            {
                if (_isLoading == value)
                {
                    return;
                }

                _isLoading = value;

                OnPropertyChanged();
            }
        }

        private void OnPropertyChanged(
            [CallerMemberName]
            string? propertyName = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(
                    propertyName));
        }
    }
}
