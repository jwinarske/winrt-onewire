﻿using System.Diagnostics.Tracing;

namespace com.dalsemi.onewire.logging
{
    public sealed class OneWireEventSource : EventSource
    {
        [Event(1, Level = EventLevel.Verbose)]
        public void Debug(string message)
        {
            if (this.IsEnabled()) this.WriteEvent(1, message);
        }

        [Event(2, Level = EventLevel.Informational)]
        public void Info(string message)
        {
            if (this.IsEnabled()) this.WriteEvent(2, message);
        }

        [Event(3, Level = EventLevel.Warning)]
        public void Warn(string message)
        {
            if (this.IsEnabled()) this.WriteEvent(3, message);
        }

        [Event(4, Level = EventLevel.Error)]
        public void Error(string message)
        {
            if (this.IsEnabled()) this.WriteEvent(4, message);
        }

        [Event(5, Level = EventLevel.Critical)]
        public void Critical(string message)
        {
            if (this.IsEnabled()) this.WriteEvent(5, message);
        }

        public static OneWireEventSource Log = new OneWireEventSource();
    }
}
