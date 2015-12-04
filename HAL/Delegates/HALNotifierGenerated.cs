//File automatically generated using robotdotnet-tools. Please do not modify.

using System;

// ReSharper disable CheckNamespace

namespace HAL.Base
{
    public partial class HALNotifier
    {
        static HALNotifier()
        {
            HAL.Initialize();
        }

        public delegate IntPtr InitializeNotifierDelegate(Action<uint, IntPtr> ProcessQueue, IntPtr param, ref int status);
        public static InitializeNotifierDelegate InitializeNotifier;

        public delegate void CleanNotifierDelegate(IntPtr notifier_pointer, ref int status);
        public static CleanNotifierDelegate CleanNotifier;

        public delegate void UpdateNotifierAlarmDelegate(IntPtr notifier_pointer, uint triggerTime, ref int status);
        public static UpdateNotifierAlarmDelegate UpdateNotifierAlarm;
    }
}