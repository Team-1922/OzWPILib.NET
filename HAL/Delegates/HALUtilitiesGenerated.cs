//File automatically generated using robotdotnet-tools. Please do not modify.

// ReSharper disable CheckNamespace

namespace HAL.Base
{
    public partial class HALUtilities
    {
        static HALUtilities()
        {
            HAL.Initialize();
        }

        public delegate void DelayTicksDelegate(int ticks);
        public static DelayTicksDelegate DelayTicks;

        public delegate void DelayMillisDelegate(double ms);
        public static DelayMillisDelegate DelayMillis;

        public delegate void DelaySecondsDelegate(double s);
        public static DelaySecondsDelegate DelaySeconds;
    }
}
