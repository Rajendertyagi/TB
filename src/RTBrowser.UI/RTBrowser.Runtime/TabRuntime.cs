using System;

namespace RTBrowser.Runtime
{
    public sealed class TabRuntime
    {
        public Guid Id { get; }

        public string Title { get; set; }

        public string Url { get; set; }

        public DateTime CreatedAt { get; }

        public DateTime LastAccessedAt { get; private set; }

        public TabState State { get; private set; }

        public bool IsPinned { get; set; }

        public bool HasAudio { get; set; }

        public bool IsLoading { get; set; }

        public TabRuntime()
        {
            Id = Guid.NewGuid();

            Title = "New Tab";
            Url = "about:blank";

            CreatedAt = DateTime.UtcNow;
            LastAccessedAt = DateTime.UtcNow;

            State = TabState.Active;
        }

        public void Activate()
        {
            State = TabState.Active;

            Touch();
        }

        public void SetVisible()
        {
            State = TabState.Visible;

            Touch();
        }

        public void SetBackground()
        {
            State = TabState.Background;
        }

        public void Sleep()
        {
            State = TabState.Sleeping;
        }

        public void Freeze()
        {
            State = TabState.Frozen;
        }

        public void Destroy()
        {
            State = TabState.Destroyed;
        }

        public void Restore()
        {
            State = TabState.Restoring;

            Touch();
        }

        public void Touch()
        {
            LastAccessedAt = DateTime.UtcNow;
        }
    }
}
