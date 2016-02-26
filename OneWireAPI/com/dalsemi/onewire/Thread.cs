﻿using System;

namespace com.dalsemi.onewire
{
    internal class Thread
    {
        internal static void Sleep (long ms)
        {
            new System.Threading.ManualResetEvent(false).WaitOne((int)ms);
        }

        internal static void yield()
        {
            new System.Threading.ManualResetEvent(false).WaitOne(0);
        }
    }
}