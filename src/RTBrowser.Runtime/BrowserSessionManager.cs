using RTBrowser.Core;

using System;
using System.Collections.Generic;
using System.Linq;

namespace RTBrowser.Runtime
{
    public sealed class BrowserSessionManager
    {
        private readonly List<TabSession> _sessions =
            new();

        public IReadOnlyList<TabSession> Sessions =>
            _sessions;

        public TabSession? ActiveSession { get; private set; }

        public event EventHandler<BrowserTabEventArgs>?
            SessionCreated;

        public event EventHandler<BrowserTabEventArgs>?
            SessionClosed;

        public event EventHandler<BrowserTabEventArgs>?
            ActiveSessionChanged;

        public void AddSession(
            TabSession session)
        {
            foreach (TabSession existing in _sessions)
            {
                existing.Deactivate();
            }

            session.Activate();

            _sessions.Add(session);

            ActiveSession = session;

            SessionCreated?.Invoke(
                this,
                new BrowserTabEventArgs(
                    session.Tab));

            ActiveSessionChanged?.Invoke(
                this,
                new BrowserTabEventArgs(
                    session.Tab));
        }

        public void SetActiveSession(
            Guid tabId)
        {
            TabSession? target =
                _sessions.FirstOrDefault(
                    x => x.Id == tabId);

            if (target == null)
            {
                return;
            }

            foreach (TabSession session in _sessions)
            {
                session.Deactivate();
            }

            target.Activate();

            ActiveSession = target;

            ActiveSessionChanged?.Invoke(
                this,
                new BrowserTabEventArgs(
                    target.Tab));
        }

        public void CloseSession(
            Guid tabId)
        {
            TabSession? target =
                _sessions.FirstOrDefault(
                    x => x.Id == tabId);

            if (target == null)
            {
                return;
            }

            bool wasActive =
                ActiveSession?.Id == tabId;

            target.Dispose();

            _sessions.Remove(target);

            SessionClosed?.Invoke(
                this,
                new BrowserTabEventArgs(
                    target.Tab));

            if (_sessions.Count == 0)
            {
                ActiveSession = null;

                return;
            }

            if (!wasActive)
            {
                return;
            }

            TabSession next =
                _sessions.Last();

            next.Activate();

            ActiveSession = next;

            ActiveSessionChanged?.Invoke(
                this,
                new BrowserTabEventArgs(
                    next.Tab));
        }

        public TabSession? GetSession(
            Guid tabId)
        {
            return _sessions.FirstOrDefault(
                x => x.Id == tabId);
        }
    }
}
