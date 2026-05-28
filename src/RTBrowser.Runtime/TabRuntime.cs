using System;

namespace RTBrowser.Runtime
{
    public sealed class TabRuntime
    {
        public Guid Id { get; } =
            Guid.NewGuid();

        public string Title { get; set; } =
            "New Tab";

        public string Url { get; set; } =
            "about:blank";

        public TabState State { get; private set; } =
            TabState.Active;

        public DateTime CreatedAt { get; } =
            DateTime.UtcNow;

        public DateTime LastAccessedAt { get; private set; } =
            DateTime.UtcNow;

        public bool IsPinned { get; set; }

        public bool IsMuted { get; set; }

        public void Activate()
        {
            State = TabState.Active;

            Touch();
        }

        public void Background()
        {
            State = TabState.Background;

            Touch();
        }

        public void Sleep()
        {
            State = TabState.Sleeping;
        }

        public void Freeze()
        {
            State = TabState.Frozen;
        }

        public void Restore()
        {
            State = TabState.Active;

            Touch();
        }

        public void Destroy()
        {
            State = TabState.Destroyed;
        }

        public void Touch()
        {
            LastAccessedAt =
                DateTime.UtcNow;
        }
    }
}
